namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path metric is in its life cycle.</summary>
public enum CareerPathMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One career path metric, with what the planners need to act on it.</summary>
public sealed class CareerPathMetric(string code, string label, string notes, int capacity, string timezone, string contactEmail, int sequence, CareerPathMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public int Capacity { get; set; } = capacity;
    public string Timezone { get; set; } = timezone;
    public string ContactEmail { get; set; } = contactEmail;
    public int Sequence { get; set; } = sequence;
    public CareerPathMetricStatus Status { get; set; } = status;

    /// <summary>Starts a career path metric in status requested.</summary>
    public static CareerPathMetric NewCareerPathMetric(string code, string label)
    {
        return new CareerPathMetric(code, label, "", 0, "", "", 0, CareerPathMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathMetricStatus next)
    {
        switch (Status)
        {
            case CareerPathMetricStatus.Requested:
                switch (next)
                {
                    case CareerPathMetricStatus.Expired:
                        return true;
                }
                return false;
            case CareerPathMetricStatus.Granted:
                return next == CareerPathMetricStatus.Revoked || next == CareerPathMetricStatus.Expired;
            case CareerPathMetricStatus.Revoked:
                return next == CareerPathMetricStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CareerPathMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"career path metric"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Sequence * (double)Capacity, 2);
    }

    public CareerPathMetric Copy()
    {
        return new CareerPathMetric(Code, Label, Notes, Capacity, Timezone, ContactEmail, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
