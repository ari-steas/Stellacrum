using Godot;

public class MirrorManager
{
    /* Forgive me for creating this monstrosity.
     * -Aristeas 9/5/2023
    */


    bool MirrorEnabled = false;
    public bool PlacingMirror = false;
    GpuParticles3D[] MirrorPlanes = new GpuParticles3D[3];
    CubeGrid currentGrid;

    public MirrorManager(GpuParticles3D X, GpuParticles3D Y, GpuParticles3D Z)
    {
        MirrorPlanes[0] = X;
        MirrorPlanes[1] = Y;
        MirrorPlanes[2] = Z;

        foreach (var plane in MirrorPlanes)
            plane.Visible = false;
    }

    /// <summary>
    /// Toggles whether all mirrors are visible.
    /// </summary>
    /// <param name="enabled"></param>
    public void SetMirrorsEnabled(bool enabled)
    {
        MirrorEnabled = enabled;
        currentGrid.MirrorEnabled = enabled;
        
        if (currentGrid != null)
            currentGrid.MirrorEnabled = enabled;

        CheckGridMirrors();

        if (!enabled)
        {
            PlacingMirror = false;
            activeMirror = MirrorMode.X;
        }
    }

    /// <summary>
    /// Toggles whether all mirrors are visible.
    /// </summary>
    /// <param name="active"></param>
    public void SetMirrorsVisible(bool visible)
    {
        for (int i = 0; i < 3; i++)
            SetMirrorVisible((MirrorMode) i, visible);
    }

    public bool GetMirrorsEnabled()
    {
        return MirrorEnabled;
    }

    /// <summary>
    /// Set current active grid. Mirroring is automatically enabled.
    /// </summary>
    /// <param name="grid"></param>
    public void SetActiveGrid(CubeGrid grid)
    {
        if (currentGrid != null)
            currentGrid.MirrorEnabled = false;

        currentGrid = grid;
        UnsetActiveMirror();
        grid.MirrorEnabled = MirrorEnabled;

        for (int i = 0; i < 3; i++)
        {
            GpuParticles3D plane = MirrorPlanes[i];

            plane.GetParent()?.RemoveChild(plane);
			grid.AddChild(plane);
        }
        CheckGridMirrors();
    }

    /// <summary>
    /// Set mirror's position on grid.
    /// </summary>
    /// <param name="mirror"></param>
    /// <param name="position"></param>
    private void MoveMirror(MirrorMode mirror, Vector3I position, bool set = true)
    {
        if (currentGrid == null)
            return;

        Vector3I center = (Vector3I) currentGrid.Size.GetCenter();
       
        switch (mirror)
        {
            case MirrorMode.X:
                MirrorPlanes[(int) mirror].Position = currentGrid.GridToLocalCoordinates(new(position.X, center.Y, center.Z));
                break;
            case MirrorMode.Y:
                MirrorPlanes[(int) mirror].Position = currentGrid.GridToLocalCoordinates(new(center.X, position.Y, center.Z));
                break;
            case MirrorMode.Z:
                MirrorPlanes[(int) mirror].Position = currentGrid.GridToLocalCoordinates(new(center.X, center.Y, position.Z));
                break;
        }

        if (!set)
            return;

        switch (mirror)
        {
            case MirrorMode.X:
                currentGrid.MirrorPosition.X = position.X;
                break;
            case MirrorMode.Y:
                currentGrid.MirrorPosition.Y = position.Y;
                break;
            case MirrorMode.Z:
                currentGrid.MirrorPosition.Z = position.Z;
                break;
        }
    }

    /// <summary>
    /// Sets the active mirror's position on grid, without actually changing the saved position.
    /// </summary>
    /// <param name="position"></param>
    public void MoveActiveMirror(Vector3I position)
    {
        if (!PlacingMirror)
            return;

        MoveMirror(activeMirror, position, false);
    }

    /// <summary>
    /// Sets whether a single mirror is visible.
    /// </summary>
    /// <param name="mirror"></param>
    private void SetMirrorVisible(MirrorMode mirror, bool visible)
    {
        MirrorPlanes[(int) mirror].Visible = visible;
    }

    /// <summary>
    /// Places a mirror on the current grid and moves it to position.
    /// </summary>
    /// <param name="mirror"></param>
    /// <param name="position"></param>
    public void PlaceGridMirror(MirrorMode mirror, Vector3I position)
    {
        MoveMirror(mirror, position);
        SetMirrorVisible(mirror, true);
        currentGrid.GridMirrors[(int) mirror] = true;
        
        GD.Print(mirror + " " + currentGrid.GridMirrors[(int) mirror]);
    }

    /// <summary>
    /// Removes a mirror on the current grid.
    /// </summary>
    /// <param name="mirror"></param>
    /// <param name="position"></param>
    public void RemoveGridMirror(MirrorMode mirror)
    {
        SetMirrorVisible(mirror, false);
        currentGrid.GridMirrors[(int) mirror] = false;

        GD.Print(mirror + " " + currentGrid.GridMirrors[(int) mirror]);
    }

    public MirrorMode activeMirror = MirrorMode.X;

    /// <summary>
    /// Sets active (i.e. holographic) mirror.
    /// </summary>
    /// <param name="mirror"></param>
    /// <param name="position"></param>
    public void SetActiveMirror(MirrorMode mirror)
    {
        SetMirrorVisible(activeMirror, false);
        activeMirror = mirror;
        SetMirrorVisible(mirror, true);
    }

    /// <summary>
    /// Unsets active (i.e. holographic) mirror.
    /// </summary>
    /// <param name="mirror"></param>
    /// <param name="position"></param>
    public void UnsetActiveMirror()
    {
        PlacingMirror = false;
        CheckGridMirrors();
        activeMirror = MirrorMode.X;
    }

    public void CheckGridMirrors()
    {
        if (currentGrid == null)
            return;

        Vector3 gridSize = 2.5f * currentGrid.Size.Size;

        for (int i = 0; i < 3; i++)
        {
            if (MirrorEnabled)
                MirrorPlanes[i].Visible = currentGrid.GridMirrors[i];
            MoveMirror((MirrorMode) i, currentGrid.MirrorPosition, false);

            Vector3 adjustedGridSize = 0.5f * gridSize + Vector3.One*2.5f;

            switch (i)
            {
                case 0:
                    adjustedGridSize.Y = 0;
                    break;
                case 1:
                    adjustedGridSize.Z = 0;
                    break;
                case 2:
                    adjustedGridSize.X = 0;
                    break;
            }

            MirrorPlanes[i].ProcessMaterial.Set("emission_box_extents", adjustedGridSize);

            MirrorPlanes[i].Amount = (int)(16 * 2.5f * (adjustedGridSize.X > 0 ? adjustedGridSize.X : 1) * (adjustedGridSize.Y > 0 ? adjustedGridSize.Y : 1) * (adjustedGridSize.Z > 0 ? adjustedGridSize.Z : 1));
        }

        if (PlacingMirror)
            MirrorPlanes[(int) activeMirror].Visible = true;
    }
}

public enum MirrorMode
{
	X = 0,
	Y = 1,
	Z = 2,
}