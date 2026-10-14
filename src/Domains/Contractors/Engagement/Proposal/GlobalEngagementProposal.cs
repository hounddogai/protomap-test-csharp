namespace Acme.Workforce.Contractors.Engagement.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global engagement proposal is in its life cycle.</summary>
public enum GlobalEngagementProposalStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A global engagement proposal as the engagement team keeps it.</summary>
public sealed class GlobalEngagementProposal(string code, string name, Dictionary<string, string> attributes, int quantity, double rating, string externalRef, string location, bool archived, GlobalEngagementProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Quantity { get; set; } = quantity;
    public double Rating { get; set; } = rating;
    public string ExternalRef { get; set; } = externalRef;
    public string Location { get; set; } = location;
    public bool Archived { get; set; } = archived;
    public GlobalEngagementProposalStatus Status { get; set; } = status;

    /// <summary>Starts a global engagement proposal in status idle.</summary>
    public static GlobalEngagementProposal NewGlobalEngagementProposal(string code, string name)
    {
        return new GlobalEngagementProposal(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            0.0,
            "",
            "",
            false,
            GlobalEngagementProposalStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalEngagementProposalStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalEngagementProposalStatus next)
    {
        switch (Status)
        {
            case GlobalEngagementProposalStatus.Idle:
                switch (next)
                {
                    case GlobalEngagementProposalStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalEngagementProposalStatus.Warming:
                switch (next)
                {
                    case GlobalEngagementProposalStatus.Ready:
                    case GlobalEngagementProposalStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalEngagementProposalStatus.Ready:
                return next == GlobalEngagementProposalStatus.Stopped;
            case GlobalEngagementProposalStatus.Draining:
                switch (next)
                {
                    case GlobalEngagementProposalStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalEngagementProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global engagement proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double EffectiveLoad()
    {
        return Math.Round(Rating + (double)Quantity, 2);
    }

    public GlobalEngagementProposal Copy()
    {
        return new GlobalEngagementProposal(
            Code,
            Name,
            Attributes,
            Quantity,
            Rating,
            ExternalRef,
            Location,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
