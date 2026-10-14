namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan override is in its life cycle.</summary>
public enum SuccessionPlanOverrideStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A succession plan override. Its code is unique within a tenant.</summary>
public sealed class SuccessionPlanOverride(string code, string name, int capacity, int revision, string locale, int createdAt, int windowDays, SuccessionPlanOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public int Revision { get; set; } = revision;
    public string Locale { get; set; } = locale;
    public int CreatedAt { get; set; } = createdAt;
    public int WindowDays { get; set; } = windowDays;
    public SuccessionPlanOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan override in status draft.</summary>
    public static SuccessionPlanOverride NewSuccessionPlanOverride(string code, string name)
    {
        return new SuccessionPlanOverride(code, name, 0, 0, "", 0, 0, SuccessionPlanOverrideStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanOverrideStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanOverrideStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanOverrideStatus.Draft:
                switch (next)
                {
                    case SuccessionPlanOverrideStatus.Closed:
                        return true;
                }
                return false;
            case SuccessionPlanOverrideStatus.Pending:
                return next == SuccessionPlanOverrideStatus.Active || next == SuccessionPlanOverrideStatus.Suspended;
            case SuccessionPlanOverrideStatus.Active:
                return next == SuccessionPlanOverrideStatus.Suspended || next == SuccessionPlanOverrideStatus.Closed;
            case SuccessionPlanOverrideStatus.Suspended:
                switch (next)
                {
                    case SuccessionPlanOverrideStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"succession plan override"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Revision * (double)CreatedAt, 2);
    }

    public SuccessionPlanOverride Copy()
    {
        return new SuccessionPlanOverride(Code, Name, Capacity, Revision, Locale, CreatedAt, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
