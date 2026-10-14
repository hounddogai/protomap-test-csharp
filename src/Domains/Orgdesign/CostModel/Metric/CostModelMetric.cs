namespace Acme.Workforce.Orgdesign.CostModel.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model metric is in its life cycle.</summary>
public enum CostModelMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One cost model metric, with what the planners need to act on it.</summary>
public sealed class CostModelMetric(string code, string name, string locale, string contactEmail, double floorValue, int batchSize, string grade, string timezone, CostModelMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public string ContactEmail { get; set; } = contactEmail;
    public double FloorValue { get; set; } = floorValue;
    public int BatchSize { get; set; } = batchSize;
    public string Grade { get; set; } = grade;
    public string Timezone { get; set; } = timezone;
    public CostModelMetricStatus Status { get; set; } = status;

    /// <summary>Starts a cost model metric in status created.</summary>
    public static CostModelMetric NewCostModelMetric(string code, string name)
    {
        return new CostModelMetric(code, name, "", "", 0.0, 0, "", "", CostModelMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelMetricStatus next)
    {
        switch (Status)
        {
            case CostModelMetricStatus.Created:
                return next == CostModelMetricStatus.Retired;
            case CostModelMetricStatus.Validated:
                return next == CostModelMetricStatus.Published;
            case CostModelMetricStatus.Published:
                switch (next)
                {
                    case CostModelMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)BatchSize - FloorValue, 2);
    }

    public CostModelMetric Copy()
    {
        return new CostModelMetric(Code, Name, Locale, ContactEmail, FloorValue, BatchSize, Grade, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
