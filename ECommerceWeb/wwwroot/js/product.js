var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Product/GetAllProducts",
            "type": "GET",
            "dataSrc": "data"
        },
        "columns": [
            // Görsel
            {
                "data": "imageUrl",
                "render": function (data, type, row) {
                    if (data && data !== "") {
                        return `<img src="${data}" alt="${row.name}" style="max-width: 60px; max-height: 60px;" class="img-thumbnail" />`;
                    } else {
                        return '<span class="text-muted small">Görsel yok</span>';
                    }
                },
                "width": "10%",
                "orderable": false
            },
            // Ürün Adı
            {
                "data": "name",
                "width": "15%"
            },
            // Marka
            {
                "data": "brand",
                "width": "10%"
            },
            // Kategori
            {
                "data": "categoryName",
                "render": function (data, type, row) {
                    if (row.parentCategoryName && row.parentCategoryName !== "") {
                        return `<span class="text-muted small">${row.parentCategoryName} /</span><br/><span>${row.subCategoryName}</span>`;
                    } else {
                        return data || '-';
                    }
                },
                "width": "12%"
            },
            // Fiyat
            {
                "data": "listPrice",
                "render": function (data, type, row) {
                    if (row.discountedPrice) {
                        return `<span class="text-decoration-line-through text-muted small">₺${data.toFixed(2)}</span><br/>
                                <span class="text-success fw-bold">₺${row.discountedPrice.toFixed(2)}</span>`;
                    } else {
                        return `₺${data.toFixed(2)}`;
                    }
                },
                "width": "10%"
            },
            // İndirim
            {
                "data": "discountRate",
                "render": function (data, type, row) {
                    if (data && data > 0) {
                        return `<span class="badge bg-danger">%${data}</span>`;
                    } else {
                        return '<span class="text-muted">-</span>';
                    }
                },
                "width": "8%"
            },
            // Stok
            {
                "data": "stockQuantity",
                "render": function (data, type, row) {
                    if (data === 0) {
                        return `<span class="text-danger fw-bold">${data}<br/><small class="text-danger">Tükendi</small></span>`;
                    } else {
                        return data;
                    }
                },
                "width": "8%"
            },
            // Renk
            {
                "data": "color",
                "render": function (data) {
                    return data || '-';
                },
                "width": "8%"
            },
            // Durum
            {
                "data": "isActive",
                "render": function (data) {
                    if (data) {
                        return '<span class="badge bg-success">Aktif</span>';
                    } else {
                        return '<span class="badge bg-secondary">Pasif</span>';
                    }
                },
                "width": "8%"
            },
            // İşlemler
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                            <a href="/Product/Upsert/${data}" class="btn btn-primary btn-sm">
                                <i class="bi bi-pencil-square"></i>
                            </a>
                            <a href="/Product/Delete/${data}" class="btn btn-danger btn-sm">
                                <i class="bi bi-trash"></i>
                            </a>
                        </div>
                    `;
                },
                "width": "11%",
                "orderable": false
            }
        ],
        // ✅ Manuel Türkçe dil ayarları (CDN'ye bağımlı değil)
        "language": {
            "sDecimal": ",",
            "sEmptyTable": "Tabloda herhangi bir veri mevcut değil",
            "sInfo": "_TOTAL_ kayıttan _START_ - _END_ arasındaki kayıtlar gösteriliyor",
            "sInfoEmpty": "Kayıt yok",
            "sInfoFiltered": "(_MAX_ kayıt içerisinden bulunan)",
            "sInfoPostFix": "",
            "sInfoThousands": ".",
            "sLengthMenu": "Sayfada _MENU_ kayıt göster",
            "sLoadingRecords": "Yükleniyor...",
            "sProcessing": "\u0130şleniyor...",
            "sSearch": "Ara:",
            "sZeroRecords": "Eşleşen kayıt bulunamadı",
            "oPaginate": {
                "sFirst": "\u0130lk",
                "sLast": "Son",
                "sNext": "Sonraki",
                "sPrevious": "Önceki"
            },
            "oAria": {
                "sSortAscending": ": artan sütun sıralamasını aktifleştir",
                "sSortDescending": ": azalan sütun sıralamasını aktifleştir"
            }
        },
        "order": [[2, 'asc'], [1, 'asc']], // Marka ve Ürün Adına göre sırala
        "responsive": true,
        "pageLength": 10,
        "lengthMenu": [[5, 10, 25, 50, -1], [5, 10, 25, 50, "Tümü"]]
    });
}