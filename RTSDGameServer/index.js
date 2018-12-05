var app = require('express')();
var server = require('http').Server(app);
var io = require('socket.io')(server);

server.listen(3000);

// var clients = {
    // 0: "pena",
    // 1: "jaska",
// };

var clients = {
};

var columns = 12;
var rows = 12;
var maxMove = 3;
var treeMax = 30;
var trees = [20,50];
var currentTurnIndex = 0;
var gameBoard = new Array(columns*rows);

var gameBoard = {}
var treeInd = 0;
for (i=0; i<columns*rows; i++) {
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

//gameBoard[49] = {"owner":0, "bees":100, "hasTree":0};

app.get('/', function(req, res) {
    res.send('response "/"');
});

io.on('connection', function(socket) {
    
    socket.on('player connect', function(data) {
        console.log(JSON.stringify(data)+' recv: player connect');
        var new_key = 0;
        for(var key in clients) {
            console.log(clients[key]);
            if (data.name == clients[key]) {
                console.log("Name already in use");
                // TODO Handling for this?
            }
            new_key = Math.max(new_key,parseInt(key)+1);
        }
            
        // Give joining player a starting army in an empty tile
        var location = Math.floor(Math.random() * Math.floor(columns*rows));
        while (gameBoard[location]["bees"] != 0) {
            location = Math.floor(Math.random() * Math.floor(columns*rows));
        }
        hasTree = gameBoard[location]["hasTree"]
        gameBoard[location] = {"owner":new_key, "bees":20, "hasTree":hasTree};
        console.log('Creating bees: '+JSON.stringify(gameBoard[location]));            
 
        clients[new_key] = data.name;
        data = {
            "players": clients,
            "map": gameBoard,
            "columns": columns,
            "rows": rows,
            "myIndex": new_key,
            "turnIndex": currentTurnIndex
        }
        socket.emit('player connected', data);
        console.log(data.name+' emit: player connected: '+JSON.stringify(data));
        
        newData = {};
        newData["tiles"] = {}
        newData["tiles"][location] = gameBoard[location];
        newData["turnIndex"] = currentTurnIndex;
        newData["players"] = clients;
    
        socket.broadcast.emit('other player connected', newData);
        console.log(data.name+' broadcast.emit: other player connected: '+JSON.stringify(newData));
                
    });
    
    socket.on('make move', function(data) {
        // console.log(currentPlayer.name+' recv: play: '+JSON.stringify(data));
        console.log(JSON.stringify(data)+' recv: player move');
        if (data.player == currentTurnIndex) {
            var fromX = parseInt(data.fromX);
            var fromY = parseInt(data.fromY);
            var toX = parseInt(data.toX);
            var toY = parseInt(data.toY);
            var bees = parseInt(data.beeCount);
            var player = parseInt(data.player);
            if (fromX > -1 && fromX < columns && toX > -1 && toX < columns && fromY > -1 && fromY < rows && toY > -1 && toY < rows) {
                if (Math.abs(fromX - toX) <= maxMove && Math.abs(fromY - toY) <= maxMove) {
                    fromIndex = fromX * columns + fromY;
                    if (gameBoard[fromIndex]["owner"] == player && gameBoard[fromIndex]["bees"] >= bees) {
                        // Move is legal, updating game state
                        gameBoard[fromIndex]["bees"] = gameBoard[fromIndex]["bees"] - bees;
                        if (gameBoard[fromIndex]["bees"] == 0) {
                            gameBoard[fromIndex]["owner"] = -1
                        }
                        
                        toIndex = toX * columns + toY;
                        if (gameBoard[toIndex]["owner"] == player) {
                            gameBoard[toIndex]["bees"] = gameBoard[toIndex]["bees"] + bees;
                        } else {
                            var remainingBees = gameBoard[toIndex]["bees"] - bees;
                            if (remainingBees == 0) {
                                gameBoard[toIndex]["bees"] = 0;
                                gameBoard[toIndex]["owner"] = -1;
                            } else if (remainingBees < 0) {
                                gameBoard[toIndex]["bees"] = -remainingBees;
                                gameBoard[toIndex]["owner"] = player;
                            }   else {
                                gameBoard[toIndex]["bees"] = remainingIndex;
                            }
                        }
                        
                        newData = {};
                        newData["tiles"] = {}
                        
                        for (i=0; i<trees.length; i++) {
                            if (gameBoard[trees[i]]["owner"] == currentTurnIndex && gameBoard[trees[i]]["bees"] < treeMax) {
                                gameBoard[trees[i]]["bees"] = gameBoard[trees[i]]["bees"] + 1;
                                newData["tiles"][trees[i]] = gameBoard[trees[i]];
                            }
                        }
                        
                        newData["tiles"][fromIndex] = gameBoard[fromIndex];
                        newData["tiles"][toIndex] = gameBoard[toIndex];
                        currentTurnIndex++;
                        currentTurnIndex = currentTurnIndex % Object.keys(clients).length;
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
        //socket.emit('play', currentPlayer);
        
        //socket.broadcast.emit('other player move', currentPlayer);
    });
    
    // socket.on('disconnect', function() {
        // console.log(currentPlayer.name+' recv: play: '+currentPlayer.name);
        // for (var i = 0; i < clients.length(); i++ ) {
            // if (clients[i] == currentPlayer.name) {
                // delete clients[i];   
            // }
        // }
        
        // socket.broadcast.emit('other player disconnected', currentPlayer);
    // });


});

console.log('--- server is running ...');