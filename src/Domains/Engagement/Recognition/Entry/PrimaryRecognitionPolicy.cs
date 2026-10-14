namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary recognition policy is in its life cycle.</summary>
public enum PrimaryRecognitionPolicyStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary recognition policy as the recognition team keeps it.</summary>
public sealed class PrimaryRecognitionPolicy(string code, string label, int version, int batchSize, int retries, string source, string ownerId, string location, PrimaryRecognitionPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public int BatchSize { get; set; } = batchSize;
    public int Retries { get; set; } = retries;
    public string Source { get; set; } = source;
    public string OwnerId { get; set; } = ownerId;
    public string Location { get; set; } = location;
    public PrimaryRecognitionPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a primary recognition policy in status draft.</summary>
    public static PrimaryRecognitionPolicy NewPrimaryRecognitionPolicy(string code, string label)
    {
        return new PrimaryRecognitionPolicy(code, label, 0, 0, 0, "", "", "", PrimaryRecognitionPolicyStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryRecognitionPolicyStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryRecognitionPolicyStatus next)
    {
        switch (Status)
        {
            case PrimaryRecognitionPolicyStatus.Draft:
                switch (next)
                {
                    case PrimaryRecognitionPolicyStatus.Active:
                    case PrimaryRecognitionPolicyStatus.Suspended:
                        return true;
                }
                return false;
            case PrimaryRecognitionPolicyStatus.Pending:
                switch (next)
                {
                    case PrimaryRecognitionPolicyStatus.Active:
                    case PrimaryRecognitionPolicyStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryRecognitionPolicyStatus.Active:
                switch (next)
                {
                    case PrimaryRecognitionPolicyStatus.Suspended:
                    case PrimaryRecognitionPolicyStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryRecognitionPolicyStatus.Suspended:
                return next == PrimaryRecognitionPolicyStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimaryRecognitionPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary recognition policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Retries * (double)Version, 2);
    }

    public PrimaryRecognitionPolicy Copy()
    {
        return new PrimaryRecognitionPolicy(
            Code,
            Label,
            Version,
            BatchSize,
            Retries,
            Source,
            OwnerId,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
