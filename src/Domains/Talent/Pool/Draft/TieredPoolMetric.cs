namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pool metric is in its life cycle.</summary>
public enum TieredPoolMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a tiered pool metric and its current status.</summary>
public sealed class TieredPoolMetric(string code, string label, string externalRef, string contactEmail, string title, bool pinned, string checksum, int revision, TieredPoolMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string ContactEmail { get; set; } = contactEmail;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public string Checksum { get; set; } = checksum;
    public int Revision { get; set; } = revision;
    public TieredPoolMetricStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pool metric in status created.</summary>
    public static TieredPoolMetric NewTieredPoolMetric(string code, string label)
    {
        return new TieredPoolMetric(code, label, "", "", "", false, "", 0, TieredPoolMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPoolMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPoolMetricStatus next)
    {
        switch (Status)
        {
            case TieredPoolMetricStatus.Created:
                switch (next)
                {
                    case TieredPoolMetricStatus.Published:
                        return true;
                }
                return false;
            case TieredPoolMetricStatus.Validated:
                switch (next)
                {
                    case TieredPoolMetricStatus.Retired:
                        return true;
                }
                return false;
            case TieredPoolMetricStatus.Published:
                switch (next)
                {
                    case TieredPoolMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredPoolMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pool metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"tiered pool metric"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public TieredPoolMetric Copy()
    {
        return new TieredPoolMetric(
            Code,
            Label,
            ExternalRef,
            ContactEmail,
            Title,
            Pinned,
            Checksum,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
