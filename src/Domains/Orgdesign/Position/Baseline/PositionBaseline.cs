namespace Acme.Workforce.Orgdesign.Position.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position baseline is in its life cycle.</summary>
public enum PositionBaselineStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One position baseline, with what the planners need to act on it.</summary>
public sealed class PositionBaseline(string code, string label, bool active, string skill, string contactEmail, double amount, PositionBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public string Skill { get; set; } = skill;
    public string ContactEmail { get; set; } = contactEmail;
    public double Amount { get; set; } = amount;
    public PositionBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a position baseline in status new.</summary>
    public static PositionBaseline NewPositionBaseline(string code, string label)
    {
        return new PositionBaseline(code, label, false, "", "", 0.0, PositionBaselineStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionBaselineStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionBaselineStatus next)
    {
        switch (Status)
        {
            case PositionBaselineStatus.New:
                return next == PositionBaselineStatus.Running;
            case PositionBaselineStatus.Queued:
                switch (next)
                {
                    case PositionBaselineStatus.Running:
                    case PositionBaselineStatus.Failed:
                        return true;
                }
                return false;
            case PositionBaselineStatus.Running:
                return next == PositionBaselineStatus.Done || next == PositionBaselineStatus.Failed;
            case PositionBaselineStatus.Done:
                return next == PositionBaselineStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PositionBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"position baseline"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PositionBaseline Copy()
    {
        return new PositionBaseline(Code, Label, Active, Skill, ContactEmail, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
