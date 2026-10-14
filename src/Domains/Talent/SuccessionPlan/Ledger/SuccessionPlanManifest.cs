namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan manifest is in its life cycle.</summary>
public enum SuccessionPlanManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A succession plan manifest as the succession plan team keeps it.</summary>
public sealed class SuccessionPlanManifest(string code, string label, string segment, string notes, string name, int createdAt, string currency, string jobCode, double score, bool archived, SuccessionPlanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string Notes { get; set; } = notes;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public string Currency { get; set; } = currency;
    public string JobCode { get; set; } = jobCode;
    public double Score { get; set; } = score;
    public bool Archived { get; set; } = archived;
    public SuccessionPlanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan manifest in status requested.</summary>
    public static SuccessionPlanManifest NewSuccessionPlanManifest(string code, string label)
    {
        return new SuccessionPlanManifest(
            code,
            label,
            "",
            "",
            "",
            0,
            "",
            "",
            0.0,
            false,
            SuccessionPlanManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanManifestStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanManifestStatus.Requested:
                return next == SuccessionPlanManifestStatus.Granted || next == SuccessionPlanManifestStatus.Expired;
            case SuccessionPlanManifestStatus.Granted:
                return next == SuccessionPlanManifestStatus.Revoked;
            case SuccessionPlanManifestStatus.Revoked:
                return next == SuccessionPlanManifestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Score + (double)CreatedAt, 2);
    }

    public SuccessionPlanManifest Copy()
    {
        return new SuccessionPlanManifest(
            Code,
            Label,
            Segment,
            Notes,
            Name,
            CreatedAt,
            Currency,
            JobCode,
            Score,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
