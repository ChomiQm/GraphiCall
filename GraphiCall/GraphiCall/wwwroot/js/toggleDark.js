function toggleDarkMode() {
    document.documentElement.classList.toggle('dark-mode');
    var isDark = document.documentElement.classList.contains('dark-mode');
    localStorage.setItem('darkMode', isDark);
}

// Dodaj ten fragment, aby za�adowa� preferowany tryb przy otwieraniu strony
document.addEventListener('DOMContentLoaded', (event) => {
    if (localStorage.getItem('darkMode') === 'true') {
        document.documentElement.classList.add('dark-mode');
    }
});
