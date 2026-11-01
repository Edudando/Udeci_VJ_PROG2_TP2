using UnityEngine;

// --- ESTADO IDLE (REPOSO) ---
public class PlayerIdleState : IState
{
    private PlayerController3D player;

    public PlayerIdleState(PlayerController3D player)
    {
        this.player = player;
    }

    public void Enter() { }

    public void Update()
    {
        if (!player.IsAlive)
        {
            player.StateMachine.ChangeState(player.DeadState);
            return;
        }

        player.HandleInputs();
        player.ApplyGravityOnly(); // Mantiene la gravedad activa en reposo

        if (player.HasMovementInput())
        {
            player.StateMachine.ChangeState(player.MoveState);
        }
    }

    public void Exit() { }
}

// --- ESTADO MOVE (MOVIMIENTO) ---
public class PlayerMoveState : IState
{
    private PlayerController3D player;

    public PlayerMoveState(PlayerController3D player)
    {
        this.player = player;
    }

    public void Enter() { }

    public void Update()
    {
        if (!player.IsAlive)
        {
            player.StateMachine.ChangeState(player.DeadState);
            return;
        }

        player.HandleInputs();
        player.Move();

        if (!player.HasMovementInput())
        {
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    public void Exit() { }
}

// --- ESTADO DEAD (MUERTO) ---
public class PlayerDeadState : IState
{
    private PlayerController3D player;

    public PlayerDeadState(PlayerController3D player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.ResetMotion();
    }

    public void Update()
    {
        // Transición de regreso si revive
        if (player.IsAlive)
        {
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    public void Exit() { }
}