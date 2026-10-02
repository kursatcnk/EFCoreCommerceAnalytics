// Sipariş formu: seçilen ürüne göre birim fiyat ve tahmini toplamı gösterir, müşteri/ürün arama pencerelerini çalıştırır.
// Arama sonuçları textContent ile yazılıyor; eskiden müşteri ve ürün adları HTML olarak sayfaya basılıyordu.
(function () {
    "use strict";

    var money = new Intl.NumberFormat("tr-TR", { style: "currency", currency: "TRY" });

    function updatePrices() {
        var product = document.getElementById("product");
        var quantity = document.getElementById("quantity");
        if (!product || !quantity) {
            return;
        }

        var option = product.options[product.selectedIndex];
        var price = parseFloat(option ? option.getAttribute("data-price") : "0") || 0;
        var count = parseInt(quantity.value, 10) || 0;

        document.getElementById("unitPrice").value = price > 0 ? money.format(price) : "";
        document.getElementById("totalPrice").value = price > 0 && count > 0 ? money.format(price * count) : "";
    }

    function bindPicker(input) {
        var url = input.getAttribute("data-picker-search");
        var select = document.getElementById(input.getAttribute("data-picker-target"));
        var results = input.parentElement.querySelector("[data-picker-results]");
        var timer = null;

        input.addEventListener("input", function () {
            window.clearTimeout(timer);
            timer = window.setTimeout(function () {
                fetch(url + "?term=" + encodeURIComponent(input.value.trim()))
                    .then(function (response) { return response.json(); })
                    .then(function (items) {
                        results.replaceChildren();
                        items.forEach(function (item) {
                            var button = document.createElement("button");
                            button.type = "button";
                            button.className = "list-group-item list-group-item-action";
                            button.textContent = item.price !== undefined
                                ? item.name + " · " + money.format(item.price)
                                : item.name + " · " + item.city;
                            button.addEventListener("click", function () {
                                select.value = String(item.id);
                                select.dispatchEvent(new Event("change"));
                                window.jQuery(input.closest(".modal")).modal("hide");
                            });
                            results.appendChild(button);
                        });
                        if (items.length === 0) {
                            var empty = document.createElement("div");
                            empty.className = "text-muted small p-2";
                            empty.textContent = "Sonuç yok.";
                            results.appendChild(empty);
                        }
                    });
            }, 250);
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        var product = document.getElementById("product");
        var quantity = document.getElementById("quantity");
        if (product) product.addEventListener("change", updatePrices);
        if (quantity) quantity.addEventListener("input", updatePrices);
        updatePrices();

        document.querySelectorAll("[data-picker-search]").forEach(bindPicker);
    });
})();
