namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional assignment exception is in its life cycle.</summary>
public enum RegionalAssignmentExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A regional assignment exception. Its code is unique within a tenant.</summary>
public sealed class RegionalAssignmentException(string code, string title, string managerId, string channel, int attempts, string jobCode, string phone, string department, int expiresAt, double budget, int batchSize, RegionalAssignmentExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public int Attempts { get; set; } = attempts;
    public string JobCode { get; set; } = jobCode;
    public string Phone { get; set; } = phone;
    public string Department { get; set; } = department;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Budget { get; set; } = budget;
    public int BatchSize { get; set; } = batchSize;
    public RegionalAssignmentExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a regional assignment exception in status draft.</summary>
    public static RegionalAssignmentException NewRegionalAssignmentException(string code, string title)
    {
        return new RegionalAssignmentException(
            code,
            title,
            "",
            "",
            0,
            "",
            "",
            "",
            0,
            0.0,
            0,
            RegionalAssignmentExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAssignmentExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAssignmentExceptionStatus next)
    {
        switch (Status)
        {
            case RegionalAssignmentExceptionStatus.Draft:
                switch (next)
                {
                    case RegionalAssignmentExceptionStatus.Closed:
                        return true;
                }
                return false;
            case RegionalAssignmentExceptionStatus.Pending:
                return next == RegionalAssignmentExceptionStatus.Closed;
            case RegionalAssignmentExceptionStatus.Active:
                return next == RegionalAssignmentExceptionStatus.Suspended;
            case RegionalAssignmentExceptionStatus.Suspended:
                switch (next)
                {
                    case RegionalAssignmentExceptionStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalAssignmentExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional assignment exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)BatchSize + (double)ExpiresAt, 2);
    }

    public RegionalAssignmentException Copy()
    {
        return new RegionalAssignmentException(
            Code,
            Title,
            ManagerId,
            Channel,
            Attempts,
            JobCode,
            Phone,
            Department,
            ExpiresAt,
            Budget,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
