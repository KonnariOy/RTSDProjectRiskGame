var app = require('express')();
var server = require('http').Server(app);
var io = require('socket.io')(server);
var PouchDB = require('pouchdb');

server.listen(3000);

var clients = {};                       // Joined clients. Structure {"index": {"name":string, "active":bool, "bees":int}}
var columns = 12;                       // Horizontal (x) dimension of map.
var rows = 12;                          // Vertical (y) dimension of map.
var mapsize = columns*rows;             // Total tile count in map.
var maxMove = 3;                        // Army can move this many tiles in x and y directions.
var treeMax = 30;                       // Army can only grow to this size in a tree tile.
var trees = [20,26,65,70,72,111,117];   // Initialize map with trees in these tiles.
var currentTurnIndex = -1;              // Only player with this index can make a move.
var maxPlayers = 4;                     // Max for one game instance TODO: Support for multiple game instances.
var gameBoard = new Array(mapsize);     // Keeps track of owner, bees and trees in all tiles.
var playerJoined = [];                  // Keeps track of players that need starting armies after joining.
var minPlayerCount = 2;                 // Game logic is paused when less players are joined.
var activePlayers = 0;                  // Currently connected players.
var accountDatabase = new PouchDB('accounts');
var accounts = {
    foo: "bar",
    baz: "bax"
};

// Initialize map with trees. Set ownership to -1 (no owner) for all tiles.

function InitializeBoard() {
    var treeInd = 0;
    for (var i=0; i<mapsize; i++) {
        if (treeInd < trees.length) {
            if (i == trees[treeInd]) {
                gameBoard[i] = {"owner":-1, "bees":0, "hasTree":1};
                treeInd++;
                console.log("Creating tree at: "+i);
            } else {
                gameBoard[i] = {"owner":-1, "bees":0, "hasTree":0};
            }
        } else {
            gameBoard[i] = {"owner":-1, "bees":0, "hasTree":0};
        }
    }
}

function addJoinedPlayers(newData) {
    // Gives bee armies to new players in stack.
    while (playerJoined.length > 0) {
        // Give joining player a starting army in an empty tile
        var location = Math.floor(Math.random() * Math.floor(mapsize));
        // TODO: ineffective and can get stuck if every tile has bees
        while (gameBoard[location]["bees"] != 0) {
            location = Math.floor(Math.random() * Math.floor(mapsize));
        }
        var index = playerJoined.pop()
        gameBoard[location]["owner"] = index;
        gameBoard[location]["bees"] = 20;
        clients[index]["bees"] = 20;
        newData["tiles"][location] = gameBoard[location];
        console.log('Creating bees: '+JSON.stringify(gameBoard[location]));
    }
}

function increaseTurnIndex() {
    // Checks whose turn is next and sets currentTurnIndex to that player's index
    firstActiveClient = true;
    var firstIndex = -1;
    for (var key in clients) {
        if (clients[key]["active"]) {
            if (firstActiveClient) {
                firstIndex = key;
                firstActiveClient = false;
            }
            if (key > currentTurnIndex) {
                currentTurnIndex = parseInt(key);
                console.log("New turn index: "+key);
                return;
            }
        }
    }
    currentTurnIndex = parseInt(firstIndex);
    console.log("Starting from beginning. New turn index: "+firstIndex);
}

function passTurn(socket, index) {
    // Pass turn without making a move, calculate increased bee counts for passing player. 
    if (index == currentTurnIndex) {
        newData = {};
        newData["tiles"] = {};
        increaseBeeCount(newData);
        increaseTurnIndex();
        newData["turnIndex"] = currentTurnIndex; 

        // If new players have joined during this turn, add their army to the map at the end of this turn
        addJoinedPlayers(newData);

        socket.emit('move ok', newData);
        console.log('emit: move ok: '+JSON.stringify(newData));
        socket.broadcast.emit('move ok', newData);
    } else {
        // TODO Block passing at client side when it's not your turn
        console.log("Attempt to pass on wrong turn: player: "+index+ " current turn: " +currentTurnIndex);
    }
}

function increaseBeeCount(newData) {
    // Grow armies at every tree owned by current player
    for (i=0; i<trees.length; i++) {
        if (gameBoard[trees[i]]["owner"] == currentTurnIndex && gameBoard[trees[i]]["bees"] < treeMax) {
            gameBoard[trees[i]]["bees"]++;
            clients[currentTurnIndex]["bees"]++;
            newData["tiles"][trees[i]] = gameBoard[trees[i]];
        }
    }
}

function checkWinCondition() {
    // Checks if fewer than 2 players are active.
    // If so, declare winner and start a new game.
    var activeClients = 0;
    for (key in clients) {
        if (clients[key]["bees"] <= 0) {
            console.log("Player "+key+" has been defeated.");
            clients[key]["active"] = false;
        }
        if (clients[key]["active"]) {
            activeClients++;
        }
    }
    if (activeClients <= 1) {
        console.log("We have a winner");
        // TODO Handle restarting game here.
    }    
}

InitializeBoard();

app.get('/', function(req, res) {
    res.send('response "/"');
});

io.on('connection', function(socket) {
    
    var currentPlayer;

    socket.on('player login', function(data) {
        console.log(JSON.stringify(data) + ' recv: player login');
        if (accounts.hasOwnProperty(data.username) && accounts[data.username] == data.password) {
            socket.emit('login ok');
        } else {
            socket.emit('login fail');
        }
    })

    socket.on('player create_account', function(data) {
        console.log(JSON.stringify(data) + ' recv: player create_account');
        accountDatabase.post({
            username: data.username,
            password: data.password
        }, function(err, response) {
            if (err) {
                return console.log(err);
            }
            if (response) {
                return console.log(response);
            }
        });
    })
    
    socket.on('player connect', function(data) {
        console.log(JSON.stringify(data)+' recv: player connect');
        if (Object.keys(clients).length == maxPlayers) {
            console.log("Player can't join. Limit reached.");
            socket.emit('lobby full');
            return;
        }
        var new_key = 0;
        for(var key in clients) {
            console.log(clients[key]);
            if (data.name == clients[key]) {
                console.log("Name already in use");
                // TODO Handling for this?
            }
            new_key = Math.max(new_key,parseInt(key)+1);
        }
            
        currentPlayer = new_key;
        playerJoined.push(new_key); // Joining player is given an army after next turn in 'make move' or now if game paused        
 
        activePlayers = 0;
        for (var key in clients) {
            if (clients[key]["active"]) {
                console.log("Player " + key + " is active.");
                activePlayers++;
            }
        }
        
        clients[new_key] = {"name":data.name, "active":true};
        
        var newData = {};
        newData["tiles"] = {};
        if (activePlayers < minPlayerCount) {
            addJoinedPlayers(newData);    
        }
        
        if (currentTurnIndex == -1) {
            currentTurnIndex = currentPlayer;
        }
        
        data = {
            "players": clients,
            "map": gameBoard,
            "columns": columns,
            "rows": rows,
            "myIndex": new_key,
            "turnIndex": currentTurnIndex
        }
        
        socket.emit('player connected', data);
        console.log(currentPlayer+' emit: player connected: '+JSON.stringify(data));
        
        newData["turnIndex"] = currentTurnIndex;
        newData["players"] = clients;
    
        socket.broadcast.emit('other player connected', newData);
        console.log('broadcast.emit: other player connected: '+JSON.stringify(newData));
                
    });
    
    socket.on('make move', function(data) {
        // console.log(currentPlayer.name+' recv: play: '+JSON.stringify(data));
        console.log(JSON.stringify(data)+' recv: player move');
        
        activePlayers = 0;
        for (var key in clients) {
            if (clients[key]["active"]) {
                console.log("Player " + key + " is active.");
                activePlayers++;
            }
        }
        if (activePlayers < minPlayerCount) {
            console.log("Not enough players. Discarding move.");
            return;
        }  
        
        if (data.player == currentTurnIndex) {
            var fromX = parseInt(data.fromX);
            var fromY = parseInt(data.fromY);
            var toX = parseInt(data.toX);
            var toY = parseInt(data.toY);
            var bees = parseInt(data.beeCount);
            var player = parseInt(data.player);
            if (fromX > -1 && fromX < columns && toX > -1 && toX < columns && fromY > -1 && fromY < rows && toY > -1 && toY < rows) {
                if (Math.abs(fromX - toX) <= maxMove && Math.abs(fromY - toY) <= maxMove) {
                    var fromIndex = fromX * columns + fromY;
                    if (gameBoard[fromIndex]["owner"] == player && gameBoard[fromIndex]["bees"] >= bees) {
                        // Move is legal, updating game state
                        gameBoard[fromIndex]["bees"] = gameBoard[fromIndex]["bees"] - bees;
                        if (gameBoard[fromIndex]["bees"] == 0) {
                            gameBoard[fromIndex]["owner"] = -1
                        }
                        
                        var toIndex = toX * columns + toY;
                        if (gameBoard[toIndex]["owner"] == player) {
                            // Combining 2 armies of current player
                            gameBoard[toIndex]["bees"] = gameBoard[toIndex]["bees"] + bees;
                        } else {
                            // Fighting army of another player
                            var otherPlayer = gameBoard[toIndex]["owner"];
                            var remainingBees = gameBoard[toIndex]["bees"] - bees;
                            if (otherPlayer != -1) {
                                clients[otherPlayer]["bees"] = clients[otherPlayer]["bees"] - Math.min(gameBoard[toIndex]["bees"], bees);
                                console.log("Player "+otherPlayer+ " bees remaining: "+clients[otherPlayer]["bees"]);
                            }
                            clients[player]["bees"] = clients[player]["bees"] - Math.min(gameBoard[toIndex]["bees"], bees);
                            console.log("Player "+player+ " bees remaining: "+clients[player]["bees"]);
                            if (remainingBees == 0) {
                                gameBoard[toIndex]["bees"] = 0;
                                gameBoard[toIndex]["owner"] = -1;
                            } else if (remainingBees < 0) {
                                gameBoard[toIndex]["bees"] = -remainingBees;
                                gameBoard[toIndex]["owner"] = player;
                            }   else {
                                gameBoard[toIndex]["bees"] = remainingBees;
                            }
                        }
                        
                        var newData = {};
                        newData["tiles"] = {}
                        
                        increaseBeeCount(newData);                        
                        addJoinedPlayers(newData);
                        
                        newData["tiles"][fromIndex] = gameBoard[fromIndex];
                        newData["tiles"][toIndex] = gameBoard[toIndex];
                        
                        checkWinCondition();
                        
                        increaseTurnIndex();
                        newData["turnIndex"] = currentTurnIndex; 
                        
                        socket.emit('move ok', newData);
                        console.log('emit: move ok: '+JSON.stringify(newData));
                        socket.broadcast.emit('move ok', newData);
                    }
                }
            }

        } else {
            console.log("Attempt to move on wrong turn: player: "+data.player+ " current turn: " +currentTurnIndex);
        }
    });
    
    socket.on('pass turn', function(data) {
        console.log(JSON.stringify(data)+' recv: pass turn');
        passTurn(socket, data.index)
    });
    
    socket.on('disconnect', function() {
        console.log('recv: disconnect player ' + currentPlayer);
        clients[currentPlayer]["active"] = false;
        checkWinCondition();
        if (currentPlayer == currentTurnIndex) {
            passTurn(socket, currentPlayer)
        }
        //socket.broadcast.emit('other player disconnected', currentPlayer);
    });

});

console.log('--- server is running ...');