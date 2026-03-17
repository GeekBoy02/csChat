/**
 * WebSocket-to-TCP Proxy Server
 * 
 * This proxy allows web browsers (which use WebSocket) to connect to csServer2 (which uses TCP).
 * The proxy transparently forwards messages between the WebSocket client and the TCP server.
 * 
 * Installation:
 *   1. Install Node.js from https://nodejs.org/
 *   2. Open terminal/PowerShell in csWebClient folder
 *   3. Run: npm install ws
 *   4. Run: node proxy.js
 *   5. Proxy will listen on ws://localhost:9090
 * 
 * Default TCP Server: localhost:8888 (csServer2)
 */

const WebSocket = require('ws');
const net = require('net');

const PROXY_PORT = 9090;
const TCP_SERVER_HOST = 'localhost';
const TCP_SERVER_PORT = 8888;

// Create WebSocket server
const wss = new WebSocket.Server({ port: PROXY_PORT });

console.log(`[PROXY] WebSocket server listening on ws://localhost:${PROXY_PORT}`);
console.log(`[PROXY] Forwarding connections to ${TCP_SERVER_HOST}:${TCP_SERVER_PORT}`);
console.log(`[PROXY] Waiting for connections...`);

wss.on('connection', (ws, req) => {
    const clientIp = req.socket.remoteAddress;
    console.log(`[PROXY] New WebSocket client connected from ${clientIp}`);

    let tcpSocket = null;
    let isConnecting = false;
    let isTcpConnected = false;
    let messageQueue = [];

    // Create TCP connection to csServer2
    tcpSocket = net.createConnection(TCP_SERVER_PORT, TCP_SERVER_HOST);

    tcpSocket.on('connect', () => {
        console.log(`[PROXY] TCP connection established to ${TCP_SERVER_HOST}:${TCP_SERVER_PORT}`);
        isConnecting = false;
        isTcpConnected = true;
        
        // Send any queued messages
        while (messageQueue.length > 0) {
            const queuedMessage = messageQueue.shift();
            console.log(`[PROXY] Sending queued message: ${queuedMessage}`);
            try {
                tcpSocket.write(queuedMessage + '\n');
            } catch (error) {
                console.error(`[PROXY] Error sending queued message: ${error.message}`);
            }
        }
    });

    // Forward messages from TCP to WebSocket
    tcpSocket.on('data', (data) => {
        const message = data.toString('utf-8').trim();
        console.log(`[PROXY] TCP -> WS: ${message}`);
        
        if (ws.readyState === WebSocket.OPEN) {
            try {
                ws.send(message);
            } catch (error) {
                console.error(`[PROXY] Error sending to WebSocket: ${error.message}`);
            }
        }
    });

    // Forward messages from WebSocket to TCP
    ws.on('message', (data) => {
        const message = data.toString('utf-8').trim();
        console.log(`[PROXY] WS -> TCP: ${message}`);
        
        if (isTcpConnected && tcpSocket && !tcpSocket.destroyed) {
            try {
                tcpSocket.write(message + '\n');
            } catch (error) {
                console.error(`[PROXY] Error sending to TCP: ${error.message}`);
            }
        } else if (tcpSocket && !tcpSocket.destroyed) {
            // Queue message if TCP connection not yet established
            console.log(`[PROXY] TCP not ready yet, queueing message: ${message}`);
            messageQueue.push(message);
        } else {
            console.warn(`[PROXY] TCP socket not available, message not sent`);
        }
    });

    // Handle WebSocket errors
    ws.on('error', (error) => {
        console.error(`[PROXY] WebSocket error: ${error.message}`);
    });

    // Handle WebSocket close
    ws.on('close', (code, reason) => {
        console.log(`[PROXY] WebSocket disconnected (code: ${code})`);
        if (tcpSocket && !tcpSocket.destroyed) {
            tcpSocket.destroy();
        }
    });

    // Handle TCP errors
    tcpSocket.on('error', (error) => {
        console.error(`[PROXY] TCP error: ${error.message}`);
        console.error(`[PROXY] Make sure csServer2 is running on ${TCP_SERVER_HOST}:${TCP_SERVER_PORT}`);
        
        if (ws.readyState === WebSocket.OPEN) {
            ws.close(1006, `TCP connection error: ${error.message}`);
        }
    });

    // Handle TCP close
    tcpSocket.on('close', () => {
        console.log(`[PROXY] TCP connection closed`);
        isTcpConnected = false;
        messageQueue = []; // Clear queue on disconnect
        if (ws.readyState === WebSocket.OPEN) {
            ws.close(1006, 'TCP connection closed');
        }
    });

    // Handle TCP end
    tcpSocket.on('end', () => {
        console.log(`[PROXY] TCP connection ended`);
        if (ws.readyState === WebSocket.OPEN) {
            ws.close(1006, 'TCP connection ended');
        }
    });
});

// Handle WebSocket server errors
wss.on('error', (error) => {
    console.error(`[PROXY] WebSocket server error: ${error.message}`);
});

console.log(`[PROXY] Ready to accept connections!`);
