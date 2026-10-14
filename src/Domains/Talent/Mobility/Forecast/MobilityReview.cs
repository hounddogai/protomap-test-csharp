namespace Acme.Workforce.Talent.Mobility.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility review is in its life cycle.</summary>
public enum MobilityReviewStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A mobility review. Its code is unique within a tenant.</summary>
public sealed class MobilityReview(string code, string label, double weight, Dictionary<string, string> attributes, double rate, string source, MobilityReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public string Source { get; set; } = source;
    public MobilityReviewStatus Status { get; set; } = status;

    /// <summary>Starts a mobility review in status created.</summary>
    public static MobilityReview NewMobilityReview(string code, string label)
    {
        return new MobilityReview(
            code,
            label,
            0.0,
            new Dictionary<string, string>(),
            0.0,
            "",
            MobilityReviewStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityReviewStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityReviewStatus next)
    {
        switch (Status)
        {
            case MobilityReviewStatus.Created:
                return next == MobilityReviewStatus.Published || next == MobilityReviewStatus.Retired;
            case MobilityReviewStatus.Validated:
                switch (next)
                {
                    case MobilityReviewStatus.Retired:
                        return true;
                }
                return false;
            case MobilityReviewStatus.Published:
                switch (next)
                {
                    case MobilityReviewStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double ProjectedTotal()
    {
        return Math.Round(Weight + Rate, 2);
    }

    public MobilityReview Copy()
    {
        return new MobilityReview(Code, Label, Weight, Attributes, Rate, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
