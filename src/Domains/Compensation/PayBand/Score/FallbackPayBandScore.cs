namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback pay band score is in its life cycle.</summary>
public enum FallbackPayBandScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback pay band score. Its code is unique within a tenant.</summary>
public sealed class FallbackPayBandScore(string code, string name, string notes, int limit, string label, double rate, string employeeId, string segment, Dictionary<string, string> attributes, FallbackPayBandScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public int Limit { get; set; } = limit;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string EmployeeId { get; set; } = employeeId;
    public string Segment { get; set; } = segment;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public FallbackPayBandScoreStatus Status { get; set; } = status;

    /// <summary>Starts a fallback pay band score in status open.</summary>
    public static FallbackPayBandScore NewFallbackPayBandScore(string code, string name)
    {
        return new FallbackPayBandScore(
            code,
            name,
            "",
            0,
            "",
            0.0,
            "",
            "",
            new Dictionary<string, string>(),
            FallbackPayBandScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPayBandScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPayBandScoreStatus next)
    {
        switch (Status)
        {
            case FallbackPayBandScoreStatus.Open:
                switch (next)
                {
                    case FallbackPayBandScoreStatus.InReview:
                        return true;
                }
                return false;
            case FallbackPayBandScoreStatus.InReview:
                switch (next)
                {
                    case FallbackPayBandScoreStatus.Archived:
                        return true;
                }
                return false;
            case FallbackPayBandScoreStatus.Resolved:
                switch (next)
                {
                    case FallbackPayBandScoreStatus.Reopened:
                        return true;
                }
                return false;
            case FallbackPayBandScoreStatus.Reopened:
                switch (next)
                {
                    case FallbackPayBandScoreStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackPayBandScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback pay band score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
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
        return Math.Round((double)Limit - Rate, 2);
    }

    public FallbackPayBandScore Copy()
    {
        return new FallbackPayBandScore(
            Code,
            Name,
            Notes,
            Limit,
            Label,
            Rate,
            EmployeeId,
            Segment,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
