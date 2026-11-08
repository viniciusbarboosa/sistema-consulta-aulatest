(function () {
    var chave = 'tema-consultas';

    function temaSalvo() {
        try {
            return localStorage.getItem(chave);
        } catch (e) {
            return null;
        }
    }

    function aplicar(tema) {
        document.documentElement.setAttribute('data-bs-theme', tema);
    }

    // Aplica o tema logo no <head>, antes de a página aparecer (evita o "piscar")
    aplicar(temaSalvo() || 'dark');

    window.alternarTema = function () {
        var atual = document.documentElement.getAttribute('data-bs-theme') === 'light' ? 'light' : 'dark';
        var novo = atual === 'dark' ? 'light' : 'dark';

        aplicar(novo);

        try {
            localStorage.setItem(chave, novo);
        } catch (e) {
        }
    };

    // A navegação das páginas de login pode recriar o <html>; reaplica o tema depois dela
    document.addEventListener('DOMContentLoaded', function () {
        if (window.Blazor && window.Blazor.addEventListener) {
            window.Blazor.addEventListener('enhancedload', function () {
                aplicar(temaSalvo() || 'dark');
            });
        }
    });
})();