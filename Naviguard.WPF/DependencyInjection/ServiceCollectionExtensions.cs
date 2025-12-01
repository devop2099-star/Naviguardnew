// Naviguard.WPF/DependencyInjection/ServiceCollectionExtensions.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Naviguard.Application.Interfaces;
using Naviguard.Application.Services;
using Naviguard.Application.UseCases.Groups;
using Naviguard.Application.UseCases.Pages;
using Naviguard.Domain.Interfaces;
using Naviguard.Infrastructure.Data;
using Naviguard.Infrastructure.ExternalServices;
using Naviguard.Infrastructure.Repositories;
using Naviguard.WPF.Services;
using Naviguard.WPF.ViewModels;
using Naviguard.WPF.Views;

namespace Naviguard.WPF.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ConnectionFactory
            services.AddSingleton(sp =>
                new ConnectionFactory(configuration));

            // Repositorios
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IPageRepository, PageRepository>();
            services.AddScoped<IUserAssignmentRepository, UserAssignmentRepository>();
            services.AddScoped<ICredentialRepository, CredentialRepository>();
            services.AddScoped<IPageCredentialRepository, PageCredentialRepository>();
            services.AddScoped<IBusinessStructureRepository, BusinessStructureRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IProxyRepository, ProxyRepository>();

            // Servicios Externos
            services.AddSingleton(sp =>
            {
                var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7164/";
                return new ApiClient(baseUrl);
            });

            services.AddScoped<ProxyManager>();

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Use Cases - Groups
            services.AddScoped<GetGroupsUseCase>();
            services.AddScoped<CreateGroupUseCase>();
            services.AddScoped<UpdateGroupUseCase>();
            services.AddScoped<DeleteGroupUseCase>();

            // Use Cases - Pages
            services.AddScoped<CreatePageUseCase>();
            services.AddScoped<UpdatePageUseCase>();

            // Services
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IUserAssignmentService, UserAssignmentService>();
            services.AddScoped<ICredentialService, CredentialService>();
            services.AddScoped<AuthenticationService>();

            return services;
        }

        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            // ViewModels
            services.AddTransient<GroupsPagesViewModel>();
            services.AddTransient<EditGroupsViewModel>();
            services.AddTransient<FilterPagesViewModel>();
            services.AddTransient<AssignUserToGroupsViewModel>();
            services.AddTransient<CredentialsUserPageViewModel>();
            services.AddTransient<MenuNaviguardViewModel>();

            // Views
            services.AddTransient<MenuMain>();
            services.AddTransient<GroupsPages>();
            services.AddTransient<EditGroups>();
            services.AddTransient<FilterPagesNav>();
            services.AddTransient<AssignUserToGroups>();

            // Services
            services.AddSingleton<NavigationService>();

            return services;
        }
    }
}