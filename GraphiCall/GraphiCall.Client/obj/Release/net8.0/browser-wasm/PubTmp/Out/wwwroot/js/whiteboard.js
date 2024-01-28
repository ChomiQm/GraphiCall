let drawingHistory = [], redoDrawingHistory = [];

function setupCanvas(canvas) {
    const dpr = window.devicePixelRatio || 1;
    const rect = canvas.getBoundingClientRect(); // Pobiera rozmiary elementu w relacji do viewportu

    // Ustawienie rzeczywistych rozmiarów rysowania na canvas
    canvas.width = rect.width * dpr;
    // Ustawienie wysokoœci z zachowaniem proporcji, na przyk³ad dla 16:9
    canvas.height = (rect.width * 9 / 16) * dpr;

    const ctx = canvas.getContext('2d');
    ctx.scale(dpr, dpr); // Skalowanie kontekstu do rozmiarów elementu canvas z uwzglêdnieniem gêstoœci pikseli
    return ctx;
}

window.initializeWhiteboard = () => {
    try {
        const canvas = document.getElementById('whiteboard');
        if (!canvas) throw new Error('Canvas loading error.');

        // Funkcja setupCanvas powinna byæ zdefiniowana poza blokiem try, aby by³a dostêpna globalnie
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
                console.error('Wyst¹pi³ b³¹d podczas odœwie¿ania canvas:', error);
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
                console.error('Wyst¹pi³ b³¹d podczas cofania rysunku:', error);
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
                console.error('Wyst¹pi³ b³¹d podczas ponawiania rysunku:', error);
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

                // Dodanie linku do dokumentu (nie jest wyœwietlany, ale potrzebny do dzia³ania)
                document.body.appendChild(a);

                // Klikniêcie linku
                a.click();

                // Oczyszczenie, usuniêcie linku z dokumentu
                document.body.removeChild(a);
            } catch (error) {
                console.error('Wyst¹pi³ b³¹d podczas pobierania pliku:', error);
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
                            console.error('B³¹d podczas wczytywania i przetwarzania pliku:', parseError);
                        }
                    };
                    reader.readAsText(file);
                }
            } catch (error) {
                console.error('B³¹d podczas obs³ugi zmiany w input file:', error);
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
                console.error('B³¹d podczas czyszczenia canvas:', error);
            }
        });

        const saveButton = document.getElementById('save');
        saveButton.addEventListener('click', () => {
            try {
                const dataURL = canvas.toDataURL('image/png');
                download(dataURL, 'whiteboard.png');
            } catch (error) {
                console.error('B³¹d podczas zapisywania canvas:', error);
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
        console.error('B³¹d podczas niszczenia whiteboard:', error);
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
    redoDrawingHistory = []; // Resetuj historiê redo
    refreshCanvas(); // Odœwie¿ canvas z nowymi danymi
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
