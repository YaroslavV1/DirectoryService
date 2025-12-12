using DirectoryService.Contracts.Positions;
using SharedService.Core.Abstractions.Commands;

namespace DirectoryService.Application.Positions.CreatePosition;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;