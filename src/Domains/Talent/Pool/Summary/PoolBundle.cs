namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool bundle is in its life cycle.</summary>
public enum PoolBundleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pool bundle and its current status.</summary>
public sealed class PoolBundle(string code, string name, string costCenter, string department, string badge, double score, PoolBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public string Department { get; set; } = department;
    public string Badge { get; set; } = badge;
    public double Score { get; set; } = score;
    public PoolBundleStatus Status { get; set; } = status;

    /// <summary>Starts a pool bundle in status created.</summary>
    public static PoolBundle NewPoolBundle(string code, string name)
    {
        return new PoolBundle(code, name, "", "", "", 0.0, PoolBundleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolBundleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolBundleStatus next)
    {
        switch (Status)
        {
            case PoolBundleStatus.Created:
                switch (next)
                {
                    case PoolBundleStatus.Published:
                    case PoolBundleStatus.Retired:
                        return true;
                }
                return false;
            case PoolBundleStatus.Validated:
                switch (next)
                {
                    case PoolBundleStatus.Published:
                    case PoolBundleStatus.Retired:
                        return true;
                }
                return false;
            case PoolBundleStatus.Published:
                return next == PoolBundleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PoolBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pool bundle"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public PoolBundle Copy()
    {
        return new PoolBundle(Code, Name, CostCenter, Department, Badge, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
