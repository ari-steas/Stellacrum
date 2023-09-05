using Godot;

public class MirrorManager
{
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

        for (int i = 0; i < 3; i++)
        {
            GpuParticles3D plane = MirrorPlanes[i];

            plane.GetParent()?.RemoveChild(plane);
			grid.AddChild(plane);

            MoveMirror((MirrorMode) i, currentGrid.MirrorPosition);
        }
    }

    /// <summary>
    /// Set mirror's position on grid.
    /// </summary>
    /// <param name="mirror"></param>
    /// <param name="position"></param>
    private void MoveMirror(MirrorMode mirror, Vector3I position)
    {
        if (currentGrid == null)
            return;
        
        MirrorPlanes[(int) mirror].Position = currentGrid.GridToLocalCoordinates(position);
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

    public void MoveActiveMirror(Vector3I position)
    {
        if (currentGrid == null || !PlacingMirror)
            return;
        
        MirrorPlanes[(int) activeMirror].Position = currentGrid.GridToLocalCoordinates(position);
        switch (activeMirror)
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
    /// Unsets active (i.e. holographic) mirror.
    /// </summary>
    /// <param name="mirror"></param>
    /// <param name="position"></param>
    public void UnsetActiveMirror()
    {
        PlacingMirror = false;
        SetMirrorVisible(activeMirror, false);
        activeMirror = MirrorMode.X;
    }

    private void CheckGridMirrors()
    {
        for (int i = 0; i < 3; i++)
            MirrorPlanes[i].Visible = currentGrid.GridMirrors[i];

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