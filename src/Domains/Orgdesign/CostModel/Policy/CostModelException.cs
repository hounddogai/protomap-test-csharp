namespace Acme.Workforce.Orgdesign.CostModel.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model exception is in its life cycle.</summary>
public enum CostModelExceptionStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One cost model exception, with what the planners need to act on it.</summary>
public sealed class CostModelException(string code, string name, string label, string locale, bool approved, string employeeId, int version, double minimum, double hours, string jobCode, List<string> tags, CostModelExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public bool Approved { get; set; } = approved;
    public string EmployeeId { get; set; } = employeeId;
    public int Version { get; set; } = version;
    public double Minimum { get; set; } = minimum;
    public double Hours { get; set; } = hours;
    public string JobCode { get; set; } = jobCode;
    public List<string> Tags { get; set; } = tags;
    public CostModelExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a cost model exception in status new.</summary>
    public static CostModelException NewCostModelException(string code, string name)
    {
        return new CostModelException(
            code,
            name,
            "",
            "",
            false,
            "",
            0,
            0.0,
            0.0,
            "",
            new List<string>(),
            CostModelExceptionStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelExceptionStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelExceptionStatus next)
    {
        switch (Status)
        {
            case CostModelExceptionStatus.New:
                return next == CostModelExceptionStatus.Running || next == CostModelExceptionStatus.Done;
            case CostModelExceptionStatus.Queued:
                switch (next)
                {
                    case CostModelExceptionStatus.Done:
                        return true;
                }
                return false;
            case CostModelExceptionStatus.Running:
                return next == CostModelExceptionStatus.Done;
            case CostModelExceptionStatus.Done:
                return next == CostModelExceptionStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CostModelExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"cost model exception"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedVolume()
    {
        return Math.Round(Minimum * Hours, 2);
    }

    public CostModelException Copy()
    {
        return new CostModelException(
            Code,
            Name,
            Label,
            Locale,
            Approved,
            EmployeeId,
            Version,
            Minimum,
            Hours,
            JobCode,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
