namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted policy ack draft is in its life cycle.</summary>
public enum WeightedPolicyAckDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A weighted policy ack draft as the policy ack team keeps it.</summary>
public sealed class WeightedPolicyAckDraft(string code, string title, double ratio, string ownerId, double weight, int batchSize, string category, WeightedPolicyAckDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string OwnerId { get; set; } = ownerId;
    public double Weight { get; set; } = weight;
    public int BatchSize { get; set; } = batchSize;
    public string Category { get; set; } = category;
    public WeightedPolicyAckDraftStatus Status { get; set; } = status;

    /// <summary>Starts a weighted policy ack draft in status created.</summary>
    public static WeightedPolicyAckDraft NewWeightedPolicyAckDraft(string code, string title)
    {
        return new WeightedPolicyAckDraft(code, title, 0.0, "", 0.0, 0, "", WeightedPolicyAckDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedPolicyAckDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedPolicyAckDraftStatus next)
    {
        switch (Status)
        {
            case WeightedPolicyAckDraftStatus.Created:
                switch (next)
                {
                    case WeightedPolicyAckDraftStatus.Published:
                    case WeightedPolicyAckDraftStatus.Retired:
                        return true;
                }
                return false;
            case WeightedPolicyAckDraftStatus.Validated:
                return next == WeightedPolicyAckDraftStatus.Published || next == WeightedPolicyAckDraftStatus.Retired;
            case WeightedPolicyAckDraftStatus.Published:
                return next == WeightedPolicyAckDraftStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WeightedPolicyAckDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted policy ack draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round((double)BatchSize + Ratio, 2);
    }

    public WeightedPolicyAckDraft Copy()
    {
        return new WeightedPolicyAckDraft(Code, Title, Ratio, OwnerId, Weight, BatchSize, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
