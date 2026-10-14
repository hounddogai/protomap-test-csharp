namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual headcount tier is in its life cycle.</summary>
public enum ManualHeadcountTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a manual headcount tier and its current status.</summary>
public sealed class ManualHeadcountTier(string code, string label, int updatedAt, string location, string employeeId, int graceDays, ManualHeadcountTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Location { get; set; } = location;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public ManualHeadcountTierStatus Status { get; set; } = status;

    /// <summary>Starts a manual headcount tier in status new.</summary>
    public static ManualHeadcountTier NewManualHeadcountTier(string code, string label)
    {
        return new ManualHeadcountTier(code, label, 0, "", "", 0, ManualHeadcountTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualHeadcountTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualHeadcountTierStatus next)
    {
        switch (Status)
        {
            case ManualHeadcountTierStatus.New:
                switch (next)
                {
                    case ManualHeadcountTierStatus.Done:
                    case ManualHeadcountTierStatus.Failed:
                        return true;
                }
                return false;
            case ManualHeadcountTierStatus.Queued:
                switch (next)
                {
                    case ManualHeadcountTierStatus.Done:
                    case ManualHeadcountTierStatus.Failed:
                        return true;
                }
                return false;
            case ManualHeadcountTierStatus.Running:
                switch (next)
                {
                    case ManualHeadcountTierStatus.Done:
                    case ManualHeadcountTierStatus.Failed:
                        return true;
                }
                return false;
            case ManualHeadcountTierStatus.Done:
                return next == ManualHeadcountTierStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ManualHeadcountTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual headcount tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)UpdatedAt + (double)GraceDays, 2);
    }

    public ManualHeadcountTier Copy()
    {
        return new ManualHeadcountTier(Code, Label, UpdatedAt, Location, EmployeeId, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
