# ChessGame

A chess application built from scratch with a custom C# chess engine and an interactive Blazor Web UI.

The project focuses on implementing chess rules, game state management, move validation, undo functionality, draw detection, and a browser-based interface that interacts directly with the game engine.

## Features

### Chess Engine

- Complete chess board representation
- White and black pieces
- Legal move generation
- Turn management
- Check detection
- Checkmate detection
- Stalemate detection
- Piece captures
- Castling
- En passant
- Pawn promotion
- Fifty-move rule
- Threefold repetition detection
- Insufficient material detection
- Draw detection
- Game state management
- Move history
- Undo support

### Special Moves

The engine supports the main special chess rules:

- Kingside castling
- Queenside castling
- Castling validation
- En passant
- Pawn promotion
- Capturing during promotion
- Undo for special moves

### Game States

The game supports the following states:

- Normal
- Check
- Checkmate
- Stalemate
- Draw

### Undo System

The project includes a move history system that allows the game to restore the previous state.

Undo correctly restores:

- Piece positions
- Captured pieces
- Castling
- En passant
- Pawn promotion
- Current player
- Game state
- Half-move clock
- Last move
- Position repetition information

## Architecture

The solution is divided into separate projects.

```text
ChessGame
│
├── ChessGame.Core
│   ├── Game logic
│   ├── Board
│   ├── Pieces
│   ├── Move validation
│   ├── Special moves
│   ├── Draw detection
│   └── Game events
│
├── ChessGame.Tests
│   ├── Move tests
│   ├── Undo tests
│   ├── Check tests
│   ├── Checkmate tests
│   ├── Draw tests
│   └── Special move tests
│
└── ChessGame.Web
    ├── Blazor Web UI
    ├── Interactive chess board
    ├── Piece selection
    ├── Legal move highlighting
    ├── Capture highlighting
    └── Game controls
```

## Technology Stack

- C#
- .NET 8
- Blazor Web App
- Interactive Server Rendering
- Razor Components
- xUnit

## Web Interface

The Blazor interface communicates directly with the chess engine.

The current implementation supports:

- Displaying the chess board
- Rendering pieces from the real game model
- Selecting pieces with the mouse
- Highlighting legal moves
- Highlighting possible captures
- Executing moves
- Switching turns
- Displaying the current player
- Displaying the current game state
- Undoing moves

The UI does not contain chess rules. All game logic remains inside `ChessGame.Core`.

```text
User Click
    │
    ▼
ChessBoard
    │
    ▼
Game.GetLegalMoves()
    │
    ▼
Legal Move Highlighting
    │
    ▼
Game.Move()
    │
    ▼
ChessGame.Core
    │
    ▼
Board Updated
    │
    ▼
Blazor UI Re-render
```

## Running the Project

### Requirements

- .NET 8 SDK
- Visual Studio 2022 or another compatible IDE

### Clone the repository

```bash
git clone https://github.com/AntResLab/ChessGame.git
cd ChessGame
```

### Run the tests

```bash
dotnet test
```

### Run the web application

```bash
dotnet run --project ChessGame.Web
```

Then open the URL displayed in the console.

The game page is available at:

```text
/game
```

## Testing

The chess engine is covered by an extensive automated test suite.

The tests include:

- Standard piece movement
- Illegal moves
- Check detection
- Checkmate
- Stalemate
- Captures
- Castling
- En passant
- Pawn promotion
- Undo functionality
- Draw by fifty-move rule
- Draw by threefold repetition
- Draw by insufficient material

The project currently includes a large set of automated tests covering the implemented chess rules and game state transitions.

## Design Principles

The project follows a simple separation of responsibilities.

### ChessGame.Core

Responsible for:

- Chess rules
- Board state
- Move validation
- Game state
- Draw detection
- History and undo

### ChessGame.Web

Responsible for:

- User interaction
- Board rendering
- Piece selection
- Move highlighting
- Calling the game engine
- Displaying game state

The Web project does not duplicate chess rules implemented in the Core project.

## Current Development Status

The chess engine and the first version of the interactive Web UI are implemented.

Current Web UI functionality:

- Interactive chess board
- Legal move highlighting
- Capture highlighting
- Real move execution
- Turn display
- Game state display
- Undo support

Planned improvements include:

- Move history panel
- Chess notation
- Check and checkmate notifications
- Draw notifications
- Pawn promotion dialog
- Improved board design
- Responsive UI
- New game button
- Player orientation
- Game persistence
- AI opponent
- Multiplayer support

## Project Goals

The main goal of this project is to build a complete chess application while keeping the chess engine independent from the user interface.

This architecture makes it possible to use the same chess engine with different clients in the future:

```text
                ChessGame.Core
                      │
        ┌─────────────┼─────────────┐
        │             │             │
        ▼             ▼             ▼
    Blazor UI      REST API      AI Player
        │             │             │
        └─────────────┴─────────────┘
```

## Roadmap

### Phase 1 — Chess Engine

- [x] Board implementation
- [x] Piece movement
- [x] Legal move validation
- [x] Check detection
- [x] Checkmate
- [x] Stalemate
- [x] Castling
- [x] En passant
- [x] Pawn promotion
- [x] Draw rules
- [x] Move history
- [x] Undo system
- [x] Automated tests

### Phase 2 — Web UI

- [x] Blazor Web application
- [x] Chess board rendering
- [x] Interactive Server mode
- [x] Piece selection
- [x] Legal move highlighting
- [x] Capture highlighting
- [x] Move execution
- [x] Turn display
- [x] Game state display
- [x] Undo

### Phase 3 — User Experience

- [ ] Move history
- [ ] Chess notation
- [ ] Check notification
- [ ] Checkmate notification
- [ ] Draw notification
- [ ] Pawn promotion dialog
- [ ] New game
- [ ] Improved visual design
- [ ] Responsive layout

### Phase 4 — Future Development

- [ ] Save and load games
- [ ] FEN support
- [ ] PGN support
- [ ] AI opponent
- [ ] REST API
- [ ] Multiplayer
- [ ] Player accounts

## License

This project is currently under active development.

---

Developed as a custom chess engine and interactive web application using C# and .NET.
