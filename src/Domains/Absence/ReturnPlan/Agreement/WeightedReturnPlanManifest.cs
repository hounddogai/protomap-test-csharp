namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted return plan manifest is in its life cycle.</summary>
public enum WeightedReturnPlanManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A weighted return plan manifest as the return plan team keeps it.</summary>
public sealed class WeightedReturnPlanManifest(string code, string name, double maximum, string employeeId, double minimum, double budget, WeightedReturnPlanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string EmployeeId { get; set; } = employeeId;
    public double Minimum { get; set; } = minimum;
    public double Budget { get; set; } = budget;
    public WeightedReturnPlanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a weighted return plan manifest in status created.</summary>
    public static WeightedReturnPlanManifest NewWeightedReturnPlanManifest(string code, string name)
    {
        return new WeightedReturnPlanManifest(
            code,
            name,
            0.0,
            "",
            0.0,
            0.0,
            WeightedReturnPlanManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedReturnPlanManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedReturnPlanManifestStatus next)
    {
        switch (Status)
        {
            case WeightedReturnPlanManifestStatus.Created:
                return next == WeightedReturnPlanManifestStatus.Published;
            case WeightedReturnPlanManifestStatus.Validated:
                return next == WeightedReturnPlanManifestStatus.Published || next == WeightedReturnPlanManifestStatus.Retired;
            case WeightedReturnPlanManifestStatus.Published:
                switch (next)
                {
                    case WeightedReturnPlanManifestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedReturnPlanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted return plan manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetTotal()
    {
        return Math.Round(Maximum - Minimum, 2);
    }

    public WeightedReturnPlanManifest Copy()
    {
        return new WeightedReturnPlanManifest(Code, Name, Maximum, EmployeeId, Minimum, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
