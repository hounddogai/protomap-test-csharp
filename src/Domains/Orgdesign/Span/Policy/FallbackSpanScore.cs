namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback span score is in its life cycle.</summary>
public enum FallbackSpanScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a fallback span score and its current status.</summary>
public sealed class FallbackSpanScore(string code, string name, string region, int graceDays, string reviewer, FallbackSpanScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int GraceDays { get; set; } = graceDays;
    public string Reviewer { get; set; } = reviewer;
    public FallbackSpanScoreStatus Status { get; set; } = status;

    /// <summary>Starts a fallback span score in status requested.</summary>
    public static FallbackSpanScore NewFallbackSpanScore(string code, string name)
    {
        return new FallbackSpanScore(code, name, "", 0, "", FallbackSpanScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackSpanScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackSpanScoreStatus next)
    {
        switch (Status)
        {
            case FallbackSpanScoreStatus.Requested:
                switch (next)
                {
                    case FallbackSpanScoreStatus.Revoked:
                    case FallbackSpanScoreStatus.Expired:
                        return true;
                }
                return false;
            case FallbackSpanScoreStatus.Granted:
                return next == FallbackSpanScoreStatus.Revoked;
            case FallbackSpanScoreStatus.Revoked:
                switch (next)
                {
                    case FallbackSpanScoreStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackSpanScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback span score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)GraceDays;
    }

    public FallbackSpanScore Copy()
    {
        return new FallbackSpanScore(Code, Name, Region, GraceDays, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
