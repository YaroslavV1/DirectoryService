using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Contracts.Positions;

namespace DirectoryService.Application.Positions.CreatePosition;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;