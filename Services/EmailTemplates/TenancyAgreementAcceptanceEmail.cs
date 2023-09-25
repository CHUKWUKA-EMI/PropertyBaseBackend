using System;
namespace PropertyBase.Services.EmailTemplates
{
    public static class TenancyAgreementAcceptanceEmail
    {
        public static string GenerateTemplate(
             string tenantName,
             string agencyName,
             string documentPageUrl
             )
        {
            return $@"
              <!DOCTYPE html>
                <html lang=""en"">
                  <head>
                    <meta charset=""UTF-8"" />
                    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                    <title>Property Inspection Request</title>
                    <style>
                      .btn {{
                        padding: 0.8rem;
                        border-radius: 0.5rem;
                        outline: none;
                        border: none;
                        font-weight: 500;
                        color: white;
                        background-color: #0b0b9f;
                        cursor: pointer;
                        font-size: 16px;
                        letter-spacing: 2px;
                        text-decoration: none;
                      }}
                      .btn:hover {{
                        background-color: #080871;
                      }}
                      .link {{
                        color: #0b0be3;
                        font-weight: bold;
                        font-size: 1.1rem;
                        
                      }}
                      
                    </style>
                    <script>
                       function openUrl(url) {{
                         window.open(url);
                         }}
                    </script>
                  </head>
                  <body>
                    <div style=""margin-right: auto; margin-left: auto"">
                      <p>Dear <b>{agencyName},</b></p>

                      <p>We hope this email finds you well. We wanted to inform you that <em>{tenantName}</em>,
                          has signed this <a class=""link"" href=""{documentPageUrl}"">Tenancy Agreement Document</a>.
                      </p>

                      <p>Warm regards,</p>

                      <p style=""color: #494747; margin-top: 2rem; font-style: italic"">
                        <b>Propery Forager Team</b>
                       
                      </p>
                    </div>
                  </body>
                </html>

            ";
        }
    }
}

