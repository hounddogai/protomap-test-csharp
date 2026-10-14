namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered certification summary is in its life cycle.</summary>
public enum TieredCertificationSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a tiered certification summary and its current status.</summary>
public sealed class TieredCertificationSummary(string code, string title, double maximum, Dictionary<string, string> attributes, int capacity, string notes, int graceDays, TieredCertificationSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Capacity { get; set; } = capacity;
    public string Notes { get; set; } = notes;
    public int GraceDays { get; set; } = graceDays;
    public TieredCertificationSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a tiered certification summary in status requested.</summary>
    public static TieredCertificationSummary NewTieredCertificationSummary(string code, string title)
    {
        return new TieredCertificationSummary(
            code,
            title,
            0.0,
            new Dictionary<string, string>(),
            0,
            "",
            0,
            TieredCertificationSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredCertificationSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredCertificationSummaryStatus next)
    {
        switch (Status)
        {
            case TieredCertificationSummaryStatus.Requested:
                return next == TieredCertificationSummaryStatus.Granted || next == TieredCertificationSummaryStatus.Expired;
            case TieredCertificationSummaryStatus.Granted:
                return next == TieredCertificationSummaryStatus.Revoked || next == TieredCertificationSummaryStatus.Expired;
            case TieredCertificationSummaryStatus.Revoked:
                return next == TieredCertificationSummaryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TieredCertificationSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered certification summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double WeightedVolume()
    {
        return Math.Round((double)Capacity * Maximum, 2);
    }

    public TieredCertificationSummary Copy()
    {
        return new TieredCertificationSummary(Code, Title, Maximum, Attributes, Capacity, Notes, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
