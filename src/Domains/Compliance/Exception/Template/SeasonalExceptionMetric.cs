namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal exception metric is in its life cycle.</summary>
public enum SeasonalExceptionMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One seasonal exception metric, with what the planners need to act on it.</summary>
public sealed class SeasonalExceptionMetric(string code, string name, bool pinned, int attempts, double ratio, string costCenter, Dictionary<string, string> attributes, string phone, List<string> tags, double score, SeasonalExceptionMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public int Attempts { get; set; } = attempts;
    public double Ratio { get; set; } = ratio;
    public string CostCenter { get; set; } = costCenter;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Phone { get; set; } = phone;
    public List<string> Tags { get; set; } = tags;
    public double Score { get; set; } = score;
    public SeasonalExceptionMetricStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal exception metric in status requested.</summary>
    public static SeasonalExceptionMetric NewSeasonalExceptionMetric(string code, string name)
    {
        return new SeasonalExceptionMetric(
            code,
            name,
            false,
            0,
            0.0,
            "",
            new Dictionary<string, string>(),
            "",
            new List<string>(),
            0.0,
            SeasonalExceptionMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalExceptionMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalExceptionMetricStatus next)
    {
        switch (Status)
        {
            case SeasonalExceptionMetricStatus.Requested:
                switch (next)
                {
                    case SeasonalExceptionMetricStatus.Granted:
                        return true;
                }
                return false;
            case SeasonalExceptionMetricStatus.Granted:
                return next == SeasonalExceptionMetricStatus.Expired;
            case SeasonalExceptionMetricStatus.Revoked:
                return next == SeasonalExceptionMetricStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SeasonalExceptionMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal exception metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal exception metric"} {Code}";
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

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetExposure()
    {
        return Math.Round((double)Attempts - Ratio, 2);
    }

    public SeasonalExceptionMetric Copy()
    {
        return new SeasonalExceptionMetric(
            Code,
            Name,
            Pinned,
            Attempts,
            Ratio,
            CostCenter,
            Attributes,
            Phone,
            Tags,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
