namespace Acme.Workforce.Orgdesign.CostModel.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model mapping is in its life cycle.</summary>
public enum CostModelMappingStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a cost model mapping and its current status.</summary>
public sealed class CostModelMapping(string code, string name, double threshold, string department, string timezone, int graceDays, CostModelMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string Department { get; set; } = department;
    public string Timezone { get; set; } = timezone;
    public int GraceDays { get; set; } = graceDays;
    public CostModelMappingStatus Status { get; set; } = status;

    /// <summary>Starts a cost model mapping in status draft.</summary>
    public static CostModelMapping NewCostModelMapping(string code, string name)
    {
        return new CostModelMapping(code, name, 0.0, "", "", 0, CostModelMappingStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelMappingStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelMappingStatus next)
    {
        switch (Status)
        {
            case CostModelMappingStatus.Draft:
                return next == CostModelMappingStatus.Closed;
            case CostModelMappingStatus.Pending:
                switch (next)
                {
                    case CostModelMappingStatus.Suspended:
                    case CostModelMappingStatus.Closed:
                        return true;
                }
                return false;
            case CostModelMappingStatus.Active:
                return next == CostModelMappingStatus.Suspended || next == CostModelMappingStatus.Closed;
            case CostModelMappingStatus.Suspended:
                switch (next)
                {
                    case CostModelMappingStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)GraceDays * Threshold, 2);
    }

    public CostModelMapping Copy()
    {
        return new CostModelMapping(Code, Name, Threshold, Department, Timezone, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
