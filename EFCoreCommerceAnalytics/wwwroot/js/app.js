// Liste sayfalarının ortak davranışları. Her view'da ayrı ayrı yazılan jQuery kodunun (ve her sayfada CDN'den
// tekrar yüklenen jQuery'nin) yerine geçiyor.
(function () {
    "use strict";

    // <input data-list-search="/Category/CategoryList" data-target="#categoryRows">
    // Yazmayı bırakınca listeyi sunucudan tekrar ister ve yalnızca tablo gövdesini değiştirir.
    function bindListSearch(input) {
        var target = document.querySelector(input.getAttribute("data-target"));
        var url = input.getAttribute("data-list-search");
        var pager = document.querySelector("[data-pager]");
        var timer = null;
        var lastQuery = input.value;

        input.addEventListener("input", function () {
            window.clearTimeout(timer);
            timer = window.setTimeout(function () {
                var query = input.value.trim();
                if (query === lastQuery) {
                    return;
                }
                lastQuery = query;

                fetch(url + "?search=" + encodeURIComponent(query), { headers: { "X-Requested-With": "XMLHttpRequest" } })
                    .then(function (response) {
                        if (!response.ok) {
                            throw new Error("HTTP " + response.status);
                        }
                        return response.text();
                    })
                    .then(function (html) {
                        target.innerHTML = html;
                        if (pager) {
                            pager.hidden = query.length > 0;
                        }
                    })
                    .catch(function (error) {
                        console.error("Liste araması başarısız:", error);
                    });
            }, 300);
        });
    }

    // <form data-confirm="Silinsin mi?"> gönderilmeden önce onay ister.
    document.addEventListener("submit", function (event) {
        var message = event.target.getAttribute && event.target.getAttribute("data-confirm");
        if (message && !window.confirm(message)) {
            event.preventDefault();
        }
    });

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll("[data-list-search]").forEach(bindListSearch);
    });
})();
