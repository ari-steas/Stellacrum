using Godot;
using System.Collections.Generic;
using Stellacrum.Data.CubeObjects;

public class GridThrustControl
{
    public bool _isUnderControl = false;
    public bool IsUnderControl
    {
        get => _isUnderControl;
        set
        {
            _isUnderControl = value;
            if (!_isUnderControl)
            {
                _linearInput = Vector3.Zero;
                _angularInput = Vector3.Zero;
            }
        }
    }

    private readonly CubeGrid _grid;
    private readonly List<ThrusterBlock> _thrusterBlocks = new();
    public bool Dampen = true;

    private Vector3 _linearInput = Vector3.Zero;
    private Vector3 _angularInput = Vector3.Zero;

    private readonly VectorPID _angularPid = new(0.5f, 0.0f, 0.1f);

    public GridThrustControl(CubeGrid grid)
    {
        _grid = grid;
        //_angularInput = grid.Rotation;

        foreach (var block in grid.GetCubeBlocks())
        {
            if (block is ThrusterBlock b)
                _thrusterBlocks.Add(b);
        }

        grid.OnBlockAdded += OnGridBlockAdded;
        grid.OnBlockRemoved += OnGridBlockRemoved;
    }

    public void Update(Vector3 linearVelocity, Vector3 angularVelocity, double delta)
    {
        Vector3 desiredLinearVel = (Dampen && _linearInput.IsZeroApprox()) ? Vector3.Zero : (_linearInput + linearVelocity);
        Vector3 desiredAngularVel = _angularPid.Update(angularVelocity, _angularInput, (float) delta);

        foreach (var thruster in _thrusterBlocks)
		{
			thruster.SetDesiredAngularVelocity(desiredAngularVel);
			thruster.SetDesiredLinearVelocity(desiredLinearVel);
		}
    }

    /// <summary>
    /// Sets linear input. Formatted as world-aligned Vector3 direct control input.
    /// </summary>
    /// <param name="input"></param>
    public void SetInputLinear(Vector3 input)
    {
        _linearInput = _grid.Basis * input;
    }

    /// <summary>
    /// Sets angular input. Formatted as world-aligned Vector3 direction.
    /// </summary>
    /// <param name="input"></param>
    public void SetInputAngular(Vector3 input)
    {
        _angularInput = input;
    }

    private void OnGridBlockAdded(CubeBlock block)
    {
        if (block is ThrusterBlock b)
            _thrusterBlocks.Add(b);
    }

    private void OnGridBlockRemoved(CubeBlock block)
    {
        if (block is ThrusterBlock b)
            _thrusterBlocks.Remove(b);
    }
}