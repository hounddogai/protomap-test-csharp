namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient scenario draft is in its life cycle.</summary>
public enum LenientScenarioDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A lenient scenario draft. Its code is unique within a tenant.</summary>
public sealed class LenientScenarioDraft(string code, string name, string title, double hours, string costCenter, string ownerId, double floorValue, string employeeId, string locale, string category, LenientScenarioDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string CostCenter { get; set; } = costCenter;
    public string OwnerId { get; set; } = ownerId;
    public double FloorValue { get; set; } = floorValue;
    public string EmployeeId { get; set; } = employeeId;
    public string Locale { get; set; } = locale;
    public string Category { get; set; } = category;
    public LenientScenarioDraftStatus Status { get; set; } = status;

    /// <summary>Starts a lenient scenario draft in status requested.</summary>
    public static LenientScenarioDraft NewLenientScenarioDraft(string code, string name)
    {
        return new LenientScenarioDraft(
            code,
            name,
            "",
            0.0,
            "",
            "",
            0.0,
            "",
            "",
            "",
            LenientScenarioDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientScenarioDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientScenarioDraftStatus next)
    {
        switch (Status)
        {
            case LenientScenarioDraftStatus.Requested:
                switch (next)
                {
                    case LenientScenarioDraftStatus.Granted:
                    case LenientScenarioDraftStatus.Revoked:
                        return true;
                }
                return false;
            case LenientScenarioDraftStatus.Granted:
                return next == LenientScenarioDraftStatus.Expired;
            case LenientScenarioDraftStatus.Revoked:
                return next == LenientScenarioDraftStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LenientScenarioDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient scenario draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient scenario draft"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(FloorValue * Hours, 2);
    }

    public LenientScenarioDraft Copy()
    {
        return new LenientScenarioDraft(
            Code,
            Name,
            Title,
            Hours,
            CostCenter,
            OwnerId,
            FloorValue,
            EmployeeId,
            Locale,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
