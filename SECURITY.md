# Security Policy

## Supported Versions

We release patches for security vulnerabilities for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |

## Reporting a Vulnerability

We take the security of Oracle Database MCP Agent seriously. If you believe you have found a security vulnerability, please report it to us as described below.

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report them privately by emailing [your-email@example.com] or by opening a private security advisory on GitHub.

You should receive a response within 48 hours. If for some reason you do not, please follow up via email to ensure we received your original message.

Please include the requested information listed below (as much as you can provide) to help us better understand the nature and scope of the possible issue:

* Type of issue (e.g. buffer overflow, SQL injection, cross-site scripting, etc.)
* Full paths of source file(s) related to the manifestation of the issue
* The location of the affected source code (tag/branch/commit or direct URL)
* Any special configuration required to reproduce the issue
* Step-by-step instructions to reproduce the issue
* Proof-of-concept or exploit code (if possible)
* Impact of the issue, including how an attacker might exploit the issue

This information will help us triage your report more quickly.

## Preferred Languages

We prefer all communications to be in English.

## Security Best Practices

When using Oracle Database MCP Agent, please follow these security best practices:

### Database Connections

- **Never commit database credentials to version control**
- Use environment variables or secure configuration management for sensitive data
- Use least-privilege database accounts with only necessary permissions
- Enable database connection encryption when possible
- Regularly rotate database passwords

### Configuration

- Store configuration files (`appsettings.json`) outside of version control
- Use secure values for connection strings
- Validate all input parameters
- Enable logging for security events

### Deployment

- Use secure networks for database connections
- Keep the .NET runtime and dependencies up to date
- Monitor for unusual database access patterns
- Implement proper access controls for the MCP server

### Example Secure Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=your-secure-host;User Id=limited-user;Password=strong-password;Encryption=true;"
  }
}
```

## Known Security Considerations

- This application executes SQL queries against your database
- Ensure proper database permissions are in place
- Monitor SQL execution for unusual patterns
- Validate input parameters in custom tools

## Updates and Patches

- Security updates will be released as soon as possible
- Check for updates regularly
- Subscribe to release notifications on GitHub

## Responsible Disclosure

We follow responsible disclosure practices and will:

1. Acknowledge receipt of your vulnerability report
2. Provide an estimated timeline for addressing the vulnerability
3. Notify you when the vulnerability is fixed
4. Credit you for the discovery (if desired)

Thank you for helping keep Oracle Database MCP Agent and our users safe!
