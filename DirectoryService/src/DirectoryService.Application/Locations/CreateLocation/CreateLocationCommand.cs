using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Contracts.Locations.CreateLocation;

namespace DirectoryService.Application.Locations.CreateLocation;

public record CreateLocationCommand(
    CreateLocationRequest Request) : ICommand;