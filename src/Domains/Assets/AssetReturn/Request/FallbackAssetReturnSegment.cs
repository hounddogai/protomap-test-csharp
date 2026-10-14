namespace Acme.Workforce.Assets.AssetReturn.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback asset return segment is in its life cycle.</summary>
public enum FallbackAssetReturnSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a fallback asset return segment and its current status.</summary>
public sealed class FallbackAssetReturnSegment(string code, string title, int endsAt, string phone, int retries, double amount, string employeeId, string description, FallbackAssetReturnSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public string Phone { get; set; } = phone;
    public int Retries { get; set; } = retries;
    public double Amount { get; set; } = amount;
    public string EmployeeId { get; set; } = employeeId;
    public string Description { get; set; } = description;
    public FallbackAssetReturnSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback asset return segment in status draft.</summary>
    public static FallbackAssetReturnSegment NewFallbackAssetReturnSegment(string code, string title)
    {
        return new FallbackAssetReturnSegment(
            code,
            title,
            0,
            "",
            0,
            0.0,
            "",
            "",
            FallbackAssetReturnSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAssetReturnSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAssetReturnSegmentStatus next)
    {
        switch (Status)
        {
            case FallbackAssetReturnSegmentStatus.Draft:
                return next == FallbackAssetReturnSegmentStatus.Active;
            case FallbackAssetReturnSegmentStatus.Pending:
                switch (next)
                {
                    case FallbackAssetReturnSegmentStatus.Active:
                    case FallbackAssetReturnSegmentStatus.Closed:
                        return true;
                }
                return false;
            case FallbackAssetReturnSegmentStatus.Active:
                return next == FallbackAssetReturnSegmentStatus.Suspended || next == FallbackAssetReturnSegmentStatus.Closed;
            case FallbackAssetReturnSegmentStatus.Suspended:
                return next == FallbackAssetReturnSegmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FallbackAssetReturnSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback asset return segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)EndsAt - Amount, 2);
    }

    public FallbackAssetReturnSegment Copy()
    {
        return new FallbackAssetReturnSegment(
            Code,
            Title,
            EndsAt,
            Phone,
            Retries,
            Amount,
            EmployeeId,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
