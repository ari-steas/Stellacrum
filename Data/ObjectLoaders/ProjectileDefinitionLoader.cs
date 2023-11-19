using Godot;
using Stellacrum.Data.CubeObjects.WeaponObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Stellacrum.Data.ObjectLoaders
{
    public class ProjectileDefinitionLoader
    {
        //private static readonly Dictionary<string, Projectile> Projectiles = new();
        private static Dictionary<string, Type> typeIds = new();
        private static Dictionary<string, Godot.Collections.Dictionary<string, Variant>> projectileDefinitions = new();
        private static readonly Dictionary<string, ProjectileBase> baseProjectiles = new();

        public static void StartLoad(string path)
        {
            GD.Print("\n\nStart Projectile load from " + path);

            projectileDefinitions.Clear();
            baseProjectiles.Clear();
            typeIds.Clear();

            if (path[^1] != '/')
                path += '/';

            List<string> allDataFiles = FileHelper.FindFilesWithExtension(path, ".json", true);

            foreach (var filePath in allDataFiles)
            {
                try
                {
                    LoadProjectilesFromFile(filePath);
                }
                catch (Exception e)
                {
                    GD.PrintErr(e);
                }
            }

            GD.Print($"Loaded {typeIds.Count} Projectiles.\n\n");
        }

        public static void Clear()
        {
            typeIds.Clear();
            projectileDefinitions.Clear();
        }

        public static string[] GetAllIds()
        {
            return typeIds.Keys.ToArray();
        }

        public static ProjectileBase ProjectileFromId(string id)
        {
            if (typeIds.ContainsKey(id))
            {
                return DefinitionLoader(id);
            }
            else
            {
                GD.PrintErr("Missing Projectile " + id);
                return baseProjectiles["HitscanTest"];
            }
        }

        /// <summary>
        /// Returns a 'base' static Projectile. Primarily used in PlaceBox.cs
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ProjectileBase ExistingBaseFromId(string id)
        {
            if (typeIds.ContainsKey(id))
            {
                return baseProjectiles[id];
            }
            else
            {
                GD.PrintErr("Missing Projectile " + id);
                return DefinitionLoader("HitscanTest");
            }
        }

        private static void LoadProjectilesFromFile(string filePath)
        {
            Json json = new();
            FileAccess infoFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

            if (json.Parse(infoFile.GetAsText()) != Error.Ok)
                throw new Exception("Unable to load Projectile @ " + filePath + " - " + json.GetErrorMessage());

            // holy mother of Godot.Collections.Dictionary
            var allData = json.Data.AsGodotDictionary<string, Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Variant>>>();

            if (allData.ContainsKey("ProjectileDefinitions"))
                foreach (var projectileDefinition in allData["ProjectileDefinitions"])
                    LoadProjectile(projectileDefinition.Key, projectileDefinition.Value);
        }

        /// <summary>
        /// Loads a single Projectile from a definition.
        /// </summary>
        /// <param name="projectileDefinition"></param>
        private static void LoadProjectile(string subTypeId, Godot.Collections.Dictionary<string, Variant> projectileData)
        {
            if (typeIds.ContainsKey(subTypeId))
                return;

            Type type = typeof(ProjectileBase);

            try
            {
                Assembly asm = typeof(ProjectileBase).Assembly;
                type = asm.GetType("Stellacrum.Data.CubeObjects.WeaponObjects." + (string)projectileData["TypeId"]);
            }
            catch
            {
                GD.PrintErr($"Missing [Type] in {subTypeId}! Setting to default...");
            }

            if (type == null)
            {
                GD.PrintErr($"{subTypeId}'s Type ({projectileData["TypeId"]}) is null!");
                return;
            }

            if (type == typeof(ProjectileBase) || type.IsSubclassOf(typeof(ProjectileBase)))
            {
                typeIds.Add(subTypeId, type);
                projectileDefinitions.Add(subTypeId, projectileData);

                GD.Print("Loaded projectile \"" + subTypeId + "\", typeof " + type.FullName + ".");
            }
            else
            {
                GD.PrintErr($"Type {type.Name} does not inherit ProjectileBase!");
            }

            baseProjectiles.Add(subTypeId, DefinitionLoader(subTypeId, true));
        }

        public static ProjectileBase LoadFromData(Godot.Collections.Dictionary<string, Variant> data)
        {
            GD.PrintErr("TODO ProjectileDefinitionLoader.cs LoadFromData for inherited types");
            ProjectileBase projectile = ProjectileFromId(data["SubTypeId"].AsString());
            projectile.Position = JsonHelper.LoadVec(data["Position"]);
            projectile.Rotation = JsonHelper.LoadVec(data["Rotation"]);

            return projectile;
        }

        /// <summary>
        /// Creates new Projectile from stored definition.
        /// </summary>
        /// <param name="subTypeId"></param>
        /// <returns></returns>
        /// <exception cref="MissingMemberException"></exception>
        private static ProjectileBase DefinitionLoader(string subTypeId, bool verbose = false)
        {
            if (!typeIds.ContainsKey(subTypeId))
                throw new MissingMemberException();

            Type type = typeIds[subTypeId];

            var projectileData = projectileDefinitions[subTypeId];

            dynamic newProjectile = Activator.CreateInstance(type, args: new object[] { subTypeId, projectileData, verbose });

            return newProjectile;
        }
    }
}
