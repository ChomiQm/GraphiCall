let drawingHistory = [], redoDrawingHistory = [];

function setupCanvas(canvas) {
    const dpr = window.devicePixelRatio || 1;
    const rect = canvas.getBoundingClientRect(); // Pobiera rozmiary elementu w relacji do viewportu

    // Ustawienie rzeczywistych rozmiar�w rysowania na canvas
    canvas.width = rect.width * dpr;
    // Ustawienie wysoko�ci z zachowaniem proporcji, na przyk�ad dla 16:9
    canvas.height = (rect.width * 9 / 16) * dpr;

    const ctx = canvas.getContext('2d');
    ctx.scale(dpr, dpr); // Skalowanie kontekstu do rozmiar�w elementu canvas z uwzgl�dnieniem g�sto�ci pikseli
    return ctx;
}

window.initializeWhiteboard = () => {
    try {
        const canvas = document.getElementById('whiteboard');
        if (!canvas) throw new Error('Canvas loading error.');

        // Funkcja setupCanvas powinna by� zdefiniowana poza blokiem try, aby by�a dost�pna globalnie
        const ctx = setupCanvas(canvas);

        let drawing = false;
        let currentPath = [];
        let currentColor = 'black', currentLineWidth = 2;

        window.addEventListener('resize', () => {
            setupCanvas(canvas);
            refreshCanvas();
        });

        function refreshCanvas() {
            try {
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                drawingHistory.forEach(path => {
                    draw(path, path.color, path.lineWidth);
                });
            } catch (error) {
                console.error('Wyst�pi� b��d podczas od�wie�ania canvas:', error);
            }
        }

        function draw(path, color, lineWidth) {
            if (path.length < 2) return; 
            ctx.beginPath();
            ctx.moveTo(path[0].x, path[0].y);
            for (let i = 1; i < path.length; i++) {
                ctx.lineTo(path[i].x, path[i].y);
            }
            ctx.strokeStyle = color;
            ctx.lineWidth = lineWidth;
            ctx.stroke();
        }

        function getMousePos(canvas, evt) {
            var rect = canvas.getBoundingClientRect();
            return {
                x: evt.clientX - rect.left,
                y: evt.clientY - rect.top
            };
        }

        canvas.addEventListener('mousedown', function (e) {
            drawing = true;
            currentPath = [getMousePos(canvas, e)]; 
        });

        canvas.addEventListener('mousemove', function (e) {
            if (drawing) {
                const newPos = getMousePos(canvas, e);
                currentPath.push(newPos); 
                draw(currentPath, currentColor, currentLineWidth); 
            }
        });

        canvas.addEventListener('mouseup', function (e) {
            if (drawing) {
                drawing = false;
                drawingHistory.push(currentPath); 
                currentPath = []; 
                refreshCanvas(); 
            }
        });

        const undoButton = document.getElementById('undo');
        undoButton.addEventListener('click', undo);
        function undo() {
            try {
                if (drawingHistory.length > 0) {
                    redoDrawingHistory.push(drawingHistory.pop());
                    refreshCanvas();
                }
            } catch (error) {
                console.error('Wyst�pi� b��d podczas cofania rysunku:', error);
            }
        }

        const redoButton = document.getElementById('redo');
        redoButton.addEventListener('click', redo);
        function redo() {
            try {
                if (redoDrawingHistory.length > 0) {
                    drawingHistory.push(redoDrawingHistory.pop());
                    refreshCanvas();
                }
            } catch (error) {
                console.error('Wyst�pi� b��d podczas ponawiania rysunku:', error);
            }
        }
     
        const saveJSONButton = document.getElementById('saveJSON');
        saveJSONButton.addEventListener('click', saveAsJSON);
        function saveAsJSON() {
            try {
                const json = JSON.stringify(drawingHistory);
                downloadJSON(json, 'whiteboard.json', 'application/json');
            } catch (error) {
                console.error('Error saving as JSON:', error);
            }
        }

        function downloadJSON(content, filename, contentType) {
            try {
                const a = document.createElement('a');
                const file = new Blob([content], { type: contentType });
                a.href = URL.createObjectURL(file);
                a.download = filename;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(a.href); // Clean up the URL object
            } catch (error) {
                console.error('Error downloading the file:', error);
            }
        }

        function download(dataURL, filename) {
            try {
                const a = document.createElement('a');

                // Ustawienie URL jako dane base64
                a.href = dataURL;

                // Ustawienie nazwy pliku
                a.download = filename;

                // Dodanie linku do dokumentu (nie jest wy�wietlany, ale potrzebny do dzia�ania)
                document.body.appendChild(a);

                // Klikni�cie linku
                a.click();

                // Oczyszczenie, usuni�cie linku z dokumentu
                document.body.removeChild(a);
            } catch (error) {
                console.error('Wyst�pi� b��d podczas pobierania pliku:', error);
            }
        }
        const loadButton = document.getElementById('load');
        loadButton.addEventListener('change', function (e) {
            try {
                const file = e.target.files[0];
                if (file) {
                    const reader = new FileReader();
                    reader.onload = function (event) {
                        try {
                            drawingHistory = JSON.parse(event.target.result);
                            redoDrawingHistory = [];
                            refreshCanvas();
                        } catch (parseError) {
                            console.error('B��d podczas wczytywania i przetwarzania pliku:', parseError);
                        }
                    };
                    reader.readAsText(file);
                }
            } catch (error) {
                console.error('B��d podczas obs�ugi zmiany w input file:', error);
            }
        });

        const clearButton = document.getElementById('clear');
        clearButton.addEventListener('click', () => {
            try {
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                drawingHistory = [];
                redoDrawingHistory = [];
                localStorage.removeItem('savedCanvas');
            } catch (error) {
                console.error('B��d podczas czyszczenia canvas:', error);
            }
        });

        const saveButton = document.getElementById('save');
        saveButton.addEventListener('click', () => {
            try {
                const dataURL = canvas.toDataURL('image/png');
                download(dataURL, 'whiteboard.png');
            } catch (error) {
                console.error('B��d podczas zapisywania canvas:', error);
            }
        });
        window.refreshCanvas = refreshCanvas;
    }
    catch (error)
    {
        console.error('An error occured while loading whiteboard script:', error);
    }
};

window.destroyWhiteboard = () => {
    try {
        const canvas = document.getElementById('whiteboard');
        if (canvas) {
            canvas.removeEventListener('mousedown', handleMouseEvents);
            canvas.removeEventListener('mouseup', handleMouseEvents);
            canvas.removeEventListener('mousemove', handleMouseEvents);
        }
    } catch (error) {
        console.error('B��d podczas niszczenia whiteboard:', error);
    }
};

window.getWhiteboardData = () => {
    console.log("Getting whiteboard data...");
    console.log(JSON.stringify(drawingHistory));
    return JSON.stringify(drawingHistory);
};

window.loadWhiteboardData = (data) => {
    console.log("Loading whiteboard data...");
    console.log(data);
    drawingHistory = JSON.parse(data);
    redoDrawingHistory = []; // Resetuj histori� redo
    refreshCanvas(); // Od�wie� canvas z nowymi danymi
};

window.loadScript = (url, callbackName) => {
    try {
        console.log(`Loading script: ${url}`);
        const script = document.createElement('script');
        script.src = url;
        script.onload = () => {
            console.log(`Script loaded: ${url}`);
            try {
                window[callbackName]();
            } catch (callbackError) {
                console.error(`Error during callback ${callbackName}:`, callbackError);
            }
        };
        document.head.appendChild(script);
    } catch (error) {
        console.error('Error loading script:', error);
    }
};
