namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment profile is in its life cycle.</summary>
public enum GarnishmentProfileStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a garnishment profile and its current status.</summary>
public sealed class GarnishmentProfile(string code, string title, Dictionary<string, string> attributes, int endsAt, int updatedAt, string externalRef, GarnishmentProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int EndsAt { get; set; } = endsAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ExternalRef { get; set; } = externalRef;
    public GarnishmentProfileStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment profile in status created.</summary>
    public static GarnishmentProfile NewGarnishmentProfile(string code, string title)
    {
        return new GarnishmentProfile(
            code,
            title,
            new Dictionary<string, string>(),
            0,
            0,
            "",
            GarnishmentProfileStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentProfileStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentProfileStatus next)
    {
        switch (Status)
        {
            case GarnishmentProfileStatus.Created:
                return next == GarnishmentProfileStatus.Published;
            case GarnishmentProfileStatus.Validated:
                switch (next)
                {
                    case GarnishmentProfileStatus.Published:
                    case GarnishmentProfileStatus.Retired:
                        return true;
                }
                return false;
            case GarnishmentProfileStatus.Published:
                return next == GarnishmentProfileStatus.Retired;
        }
        return false;
    }

    public void MoveTo(GarnishmentProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double EffectiveExposure()
    {
        return Math.Round((double)UpdatedAt - (double)EndsAt, 2);
    }

    public GarnishmentProfile Copy()
    {
        return new GarnishmentProfile(Code, Title, Attributes, EndsAt, UpdatedAt, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
