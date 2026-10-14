namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception policy is in its life cycle.</summary>
public enum ExceptionPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a exception policy and its current status.</summary>
public sealed class ExceptionPolicy(string code, string name, string label, int revision, double floorValue, ExceptionPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public double FloorValue { get; set; } = floorValue;
    public ExceptionPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a exception policy in status new.</summary>
    public static ExceptionPolicy NewExceptionPolicy(string code, string name)
    {
        return new ExceptionPolicy(code, name, "", 0, 0.0, ExceptionPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionPolicyStatus next)
    {
        switch (Status)
        {
            case ExceptionPolicyStatus.New:
                return next == ExceptionPolicyStatus.Done;
            case ExceptionPolicyStatus.Queued:
                switch (next)
                {
                    case ExceptionPolicyStatus.Running:
                        return true;
                }
                return false;
            case ExceptionPolicyStatus.Running:
                return next == ExceptionPolicyStatus.Done || next == ExceptionPolicyStatus.Failed;
            case ExceptionPolicyStatus.Done:
                return next == ExceptionPolicyStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ExceptionPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"exception policy"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(FloorValue - (double)Revision, 2);
    }

    public ExceptionPolicy Copy()
    {
        return new ExceptionPolicy(Code, Name, Label, Revision, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
