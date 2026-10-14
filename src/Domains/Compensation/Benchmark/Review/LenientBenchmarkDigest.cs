namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient benchmark digest is in its life cycle.</summary>
public enum LenientBenchmarkDigestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient benchmark digest. Its code is unique within a tenant.</summary>
public sealed class LenientBenchmarkDigest(string code, string title, double floorValue, double rate, List<string> tags, string source, string skill, string segment, LenientBenchmarkDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public double Rate { get; set; } = rate;
    public List<string> Tags { get; set; } = tags;
    public string Source { get; set; } = source;
    public string Skill { get; set; } = skill;
    public string Segment { get; set; } = segment;
    public LenientBenchmarkDigestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient benchmark digest in status created.</summary>
    public static LenientBenchmarkDigest NewLenientBenchmarkDigest(string code, string title)
    {
        return new LenientBenchmarkDigest(
            code,
            title,
            0.0,
            0.0,
            new List<string>(),
            "",
            "",
            "",
            LenientBenchmarkDigestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBenchmarkDigestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBenchmarkDigestStatus next)
    {
        switch (Status)
        {
            case LenientBenchmarkDigestStatus.Created:
                switch (next)
                {
                    case LenientBenchmarkDigestStatus.Validated:
                        return true;
                }
                return false;
            case LenientBenchmarkDigestStatus.Validated:
                return next == LenientBenchmarkDigestStatus.Published || next == LenientBenchmarkDigestStatus.Retired;
            case LenientBenchmarkDigestStatus.Published:
                switch (next)
                {
                    case LenientBenchmarkDigestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBenchmarkDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient benchmark digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double EffectiveValue()
    {
        return Math.Round(FloorValue - Rate, 2);
    }

    public LenientBenchmarkDigest Copy()
    {
        return new LenientBenchmarkDigest(Code, Title, FloorValue, Rate, Tags, Source, Skill, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
