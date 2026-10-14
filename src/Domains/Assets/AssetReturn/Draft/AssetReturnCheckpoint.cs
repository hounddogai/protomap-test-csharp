namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return checkpoint is in its life cycle.</summary>
public enum AssetReturnCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A asset return checkpoint as the asset return team keeps it.</summary>
public sealed class AssetReturnCheckpoint(string code, string name, string reviewer, double budget, int leaveDays, string employeeId, double rate, AssetReturnCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public double Budget { get; set; } = budget;
    public int LeaveDays { get; set; } = leaveDays;
    public string EmployeeId { get; set; } = employeeId;
    public double Rate { get; set; } = rate;
    public AssetReturnCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a asset return checkpoint in status draft.</summary>
    public static AssetReturnCheckpoint NewAssetReturnCheckpoint(string code, string name)
    {
        return new AssetReturnCheckpoint(code, name, "", 0.0, 0, "", 0.0, AssetReturnCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnCheckpointStatus next)
    {
        switch (Status)
        {
            case AssetReturnCheckpointStatus.Draft:
                switch (next)
                {
                    case AssetReturnCheckpointStatus.Active:
                        return true;
                }
                return false;
            case AssetReturnCheckpointStatus.Pending:
                return next == AssetReturnCheckpointStatus.Closed;
            case AssetReturnCheckpointStatus.Active:
                return next == AssetReturnCheckpointStatus.Suspended || next == AssetReturnCheckpointStatus.Closed;
            case AssetReturnCheckpointStatus.Suspended:
                switch (next)
                {
                    case AssetReturnCheckpointStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"asset return checkpoint"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Rate * Budget, 2);
    }

    public AssetReturnCheckpoint Copy()
    {
        return new AssetReturnCheckpoint(Code, Name, Reviewer, Budget, LeaveDays, EmployeeId, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
