namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount exception is in its life cycle.</summary>
public enum HeadcountExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One headcount exception, with what the planners need to act on it.</summary>
public sealed class HeadcountException(string code, string name, string label, string team, int endsAt, int version, double ceiling, string costCenter, HeadcountExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public int EndsAt { get; set; } = endsAt;
    public int Version { get; set; } = version;
    public double Ceiling { get; set; } = ceiling;
    public string CostCenter { get; set; } = costCenter;
    public HeadcountExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a headcount exception in status idle.</summary>
    public static HeadcountException NewHeadcountException(string code, string name)
    {
        return new HeadcountException(code, name, "", "", 0, 0, 0.0, "", HeadcountExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountExceptionStatus next)
    {
        switch (Status)
        {
            case HeadcountExceptionStatus.Idle:
                return next == HeadcountExceptionStatus.Warming || next == HeadcountExceptionStatus.Ready;
            case HeadcountExceptionStatus.Warming:
                return next == HeadcountExceptionStatus.Draining;
            case HeadcountExceptionStatus.Ready:
                return next == HeadcountExceptionStatus.Draining || next == HeadcountExceptionStatus.Stopped;
            case HeadcountExceptionStatus.Draining:
                return next == HeadcountExceptionStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(HeadcountExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"headcount exception"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Version * (double)EndsAt, 2);
    }

    public HeadcountException Copy()
    {
        return new HeadcountException(Code, Name, Label, Team, EndsAt, Version, Ceiling, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
