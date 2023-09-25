using System;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Newtonsoft.Json;
using PropertyBase.Contracts;
using PropertyBase.DTOs.Email;
using PropertyBase.DTOs.TenancyAgreement;
using PropertyBase.Entities;
using PropertyBase.Exceptions;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace PropertyBase.Routes
{
    public static class TenancyAgreementRoutes
    {
        public static RouteGroupBuilder DocumentApi( this RouteGroupBuilder group)
        {
            group.MapPost("/create", async (HttpContext httpContext,
                ITenancyAgreementRepository tenancyAgreementRepository,
                IUserRepository userRepository,
                IFileStorageService fileStorageService,
                IEmailService emailService
                ) =>
            {
                var form = httpContext.Request.Form;

                if (string.IsNullOrEmpty(form["propertyId"]))
                {
                    throw new RequestException(StatusCodes.Status400BadRequest, "propertyId is required.");
                }

                if(string.IsNullOrEmpty(form["propertyOwnerId"]) && string.IsNullOrEmpty(form["agencyId"]))
                {
                    throw new RequestException(StatusCodes.Status400BadRequest, "You must specify either the propertyOwnerId or the agencyId.");
                }

                if (string.IsNullOrEmpty(form["tenantId"]))
                {
                    throw new RequestException(StatusCodes.Status400BadRequest, "tenantId is required.");
                }

                var tenant = await userRepository.GetQueryable()
                              .Where(c => c.Id == form["tenantId"])
                              .FirstOrDefaultAsync();

                if (tenant == null)
                {
                    throw new RequestException(StatusCodes.Status400BadRequest, $"tenant with Id {form["tenantId"]} does not exist.");
                }

                var tenancyAgreement = await tenancyAgreementRepository.GetQueryable()
                                        .Where(c => c.TenantId == form["tenantId"] &&
                                        c.PropertyId == Guid.Parse(form["propertyId"]!))
                                        .FirstOrDefaultAsync();

                if (tenancyAgreement != null)
                {
                    throw new RequestException(StatusCodes.Status400BadRequest, "This Tenancy Agreement already exists.");
                }

                if (form.Files.Count == 0)
                {
                    throw new RequestException(StatusCodes.Status400BadRequest, "file is required.");
                }

                if (form.Files[0].Name != "file")
                {
                    throw new RequestException(StatusCodes.Status400BadRequest, "file is required.");
                }

                var uploadedFile = await fileStorageService.Upload(form.Files[0],ImageStorageFolder.Documents);
                var agreement = new TenancyAgreement
                {
                    AgencyId = !string.IsNullOrEmpty(form["agencyId"]) ? Guid.Parse(form["agencyId"]!):null,
                    documentUrl = uploadedFile.url,
                    FileId = uploadedFile.fileId,
                    PropertyId = Guid.Parse(form["propertyId"]!),
                    TenantId = form["tenantId"]!,
                    PropertyOwnerId = form["propertyOwnerId"]
                };
                var newAgreement = await tenancyAgreementRepository.AddAsync(agreement);

                var sender = new EmailUser("Property Forager Team", Environment.GetEnvironmentVariable("SUPPORT_EMAIL")!);
                var recipient = new EmailUser($"{tenant.FirstName}", tenant.Email!);
                var emailBody = emailService.GenerateHtmlForTenancyAgreementCreation($"{tenant.FirstName} {tenant.LastName}", newAgreement.Id, Guid.Parse(form["propertyId"]!));
                var emailRequest = new EmailRequest(sender, recipient, "It's time to sign your Tenancy Agreement.", emailBody);

                emailService.sendMail(emailRequest);
                return Results.Ok(newAgreement);
            });

            group.MapPut("/{documentId}/sign", async (Guid documentId,
                [FromBody] SignTenancyAgreement request,
                [FromServices] ITenancyAgreementRepository tenancyAgreementRepository,
                [FromServices] IUserRepository userRepository,
                [FromServices] IAgencyRepository agencyRepository,
                [FromServices] IEmailService emailService,
                [FromServices] ILoggedInUserService loggedInUserService
                ) =>
            {

                var tenancyAgreement = await tenancyAgreementRepository.GetByIdAsync(documentId);

                if (tenancyAgreement == null)
                {
                    throw new RequestException(StatusCodes.Status400BadRequest, $"Tenancy Agreement with id {documentId} does not exist.");
                }

                if(tenancyAgreement.TenantId != loggedInUserService.UserId)
                {
                    throw new RequestException(StatusCodes.Status400BadRequest, "You are not authorized to sign this agreement because it was not sent to you");
                }

                tenancyAgreement.Signed = true;
                tenancyAgreement.SignedByEmail = loggedInUserService.UserEmail;
                tenancyAgreement.SignedDate = DateTime.UtcNow;

                await tenancyAgreementRepository.SaveChangesAsync();
                if (tenancyAgreement.AgencyId.HasValue)
                {
                    var agency = await agencyRepository.GetQueryable()
                                       .Include(c=>c.Owner)
                                       .Where(c => c.Id == tenancyAgreement.AgencyId)
                                       .FirstOrDefaultAsync();

                    var sender = new EmailUser("Property Forager Team", Environment.GetEnvironmentVariable("SUPPORT_EMAIL")!);
                    var recipient = new EmailUser($"{agency!.AgencyName}", agency.Owner.Email!);
                    var emailBody = emailService.GenerateHtmlForTenancyAgreementAcceptance($"{request.FullName}", agency.AgencyName!, tenancyAgreement.Id);
                    var emailRequest = new EmailRequest(sender, recipient, "Tenancy Agreement has been signed!", emailBody);

                    emailService.sendMail(emailRequest);
                }
                else
                {
                    var propertyOwner = await userRepository.GetQueryable()
                                              .Where(c => c.Id == tenancyAgreement.PropertyOwnerId)
                                              .FirstOrDefaultAsync();

                    var sender = new EmailUser("Property Forager Team", Environment.GetEnvironmentVariable("SUPPORT_EMAIL")!);
                    var recipient = new EmailUser($"{propertyOwner!.FirstName}", propertyOwner.Email!);
                    var emailBody = emailService.GenerateHtmlForTenancyAgreementAcceptance($"{request.FullName}", propertyOwner.FirstName, tenancyAgreement.Id);
                    var emailRequest = new EmailRequest(sender, recipient, "Tenancy Agreement has been signed!", emailBody);
                    emailService.sendMail(emailRequest);
                }

                return Results.Ok(new {Message="Success"});

            });

            group.MapGet("/getDocuments", async (
              [FromQuery] Guid? AgencyId,
              [FromServices] ITenancyAgreementRepository tenancyAgreementRepository,
              [FromServices] ILoggedInUserService loggedInUserService
              ) =>
            {
                var tenancyAgreementDocuments = await tenancyAgreementRepository.GetQueryable()
                                          .Where(c => c.TenantId == loggedInUserService.UserId ||
                                          c.PropertyOwnerId == loggedInUserService.UserId ||
                                          c.AgencyId == AgencyId)
                                          .AsNoTracking()
                                          .ToListAsync();

                return Results.Ok(new { Message = "Success", Data = tenancyAgreementDocuments });
            });

            group.MapGet("/{documentId}", async (
               Guid documentId,
               [FromServices] ITenancyAgreementRepository tenancyAgreementRepository,
               [FromServices] ILoggedInUserService loggedInUserService
               ) =>
            {
                var tenancyAgreement = await tenancyAgreementRepository
                                                     .GetByIdAsync(documentId);

                if (tenancyAgreement == null)
                {
                    throw new RequestException(StatusCodes.Status400BadRequest, $"Tenancy Agreement with id {documentId} does not exist.");
                }

                return Results.Ok(new { Message = "Success", Data = tenancyAgreement });
            });

            group.MapDelete("/{documentId}", async (
               Guid documentId,
               [FromServices] ITenancyAgreementRepository tenancyAgreementRepository,
               [FromServices] ILoggedInUserService loggedInUserService
               ) =>
            {
                var tenancyAgreement = await tenancyAgreementRepository
                                                     .GetByIdAsync(documentId);

                if (tenancyAgreement == null)
                {
                    throw new RequestException(StatusCodes.Status400BadRequest, $"Tenancy Agreement with id {documentId} does not exist.");
                }

                if(tenancyAgreement.CreatedByUserId != loggedInUserService.UserId)
                {
                    throw new RequestException(StatusCodes.Status403Forbidden, "You cannot delete a document that wasn't created by you.");
                }

                if (tenancyAgreement.Signed)
                {
                    throw new RequestException(StatusCodes.Status403Forbidden, "You cannot delete a document that has been signed already. Kindly contact the Support Team.");
                }

                await tenancyAgreementRepository.DeleteAsync(tenancyAgreement);

                return Results.Ok(new { Message = "Success" });
            });

            return group;
        }
    }
}

