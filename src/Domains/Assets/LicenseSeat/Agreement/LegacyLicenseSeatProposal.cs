namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy license seat proposal is in its life cycle.</summary>
public enum LegacyLicenseSeatProposalStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A legacy license seat proposal. Its code is unique within a tenant.</summary>
public sealed class LegacyLicenseSeatProposal(string code, string name, int limit, Dictionary<string, string> attributes, double margin, string category, string ownerId, string locale, int priority, bool pinned, double ceiling, LegacyLicenseSeatProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Margin { get; set; } = margin;
    public string Category { get; set; } = category;
    public string OwnerId { get; set; } = ownerId;
    public string Locale { get; set; } = locale;
    public int Priority { get; set; } = priority;
    public bool Pinned { get; set; } = pinned;
    public double Ceiling { get; set; } = ceiling;
    public LegacyLicenseSeatProposalStatus Status { get; set; } = status;

    /// <summary>Starts a legacy license seat proposal in status created.</summary>
    public static LegacyLicenseSeatProposal NewLegacyLicenseSeatProposal(string code, string name)
    {
        return new LegacyLicenseSeatProposal(
            code,
            name,
            0,
            new Dictionary<string, string>(),
            0.0,
            "",
            "",
            "",
            0,
            false,
            0.0,
            LegacyLicenseSeatProposalStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyLicenseSeatProposalStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyLicenseSeatProposalStatus next)
    {
        switch (Status)
        {
            case LegacyLicenseSeatProposalStatus.Created:
                return next == LegacyLicenseSeatProposalStatus.Validated || next == LegacyLicenseSeatProposalStatus.Published;
            case LegacyLicenseSeatProposalStatus.Validated:
                return next == LegacyLicenseSeatProposalStatus.Published;
            case LegacyLicenseSeatProposalStatus.Published:
                return next == LegacyLicenseSeatProposalStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LegacyLicenseSeatProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy license seat proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy license seat proposal"} {Code}";
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

    public double EffectiveTotal()
    {
        return Math.Round((double)Priority - Margin, 2);
    }

    public LegacyLicenseSeatProposal Copy()
    {
        return new LegacyLicenseSeatProposal(
            Code,
            Name,
            Limit,
            Attributes,
            Margin,
            Category,
            OwnerId,
            Locale,
            Priority,
            Pinned,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
