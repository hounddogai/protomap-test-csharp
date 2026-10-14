namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return agreement is in its life cycle.</summary>
public enum AssetReturnAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a asset return agreement and its current status.</summary>
public sealed class AssetReturnAgreement(string code, string name, string costCenter, string team, List<string> tags, string contactEmail, double amount, AssetReturnAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public string Team { get; set; } = team;
    public List<string> Tags { get; set; } = tags;
    public string ContactEmail { get; set; } = contactEmail;
    public double Amount { get; set; } = amount;
    public AssetReturnAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a asset return agreement in status created.</summary>
    public static AssetReturnAgreement NewAssetReturnAgreement(string code, string name)
    {
        return new AssetReturnAgreement(
            code,
            name,
            "",
            "",
            new List<string>(),
            "",
            0.0,
            AssetReturnAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnAgreementStatus next)
    {
        switch (Status)
        {
            case AssetReturnAgreementStatus.Created:
                switch (next)
                {
                    case AssetReturnAgreementStatus.Published:
                        return true;
                }
                return false;
            case AssetReturnAgreementStatus.Validated:
                switch (next)
                {
                    case AssetReturnAgreementStatus.Published:
                    case AssetReturnAgreementStatus.Retired:
                        return true;
                }
                return false;
            case AssetReturnAgreementStatus.Published:
                switch (next)
                {
                    case AssetReturnAgreementStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double MeasuredValue()
    {
        return Amount;
    }

    public AssetReturnAgreement Copy()
    {
        return new AssetReturnAgreement(Code, Name, CostCenter, Team, Tags, ContactEmail, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
