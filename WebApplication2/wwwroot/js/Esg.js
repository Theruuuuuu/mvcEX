var datatable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    datatable = $('#tbltable').DataTable({
        /*取得資料*/
        "ajax": {
            "url": "/Customer/Esg/GetAll"
        },
        /*columns通常會是陣列*/
        "columns": [
            /*須注意變數名稱要和Json格式的變數名稱一致*/
            { "data": "companyNumber", "width": "15%" },
            { "data": "companyName", "width": "15%" },
            { "data": "susESG", "width": "15%" },
            { "data": "msciESG", "width": "15%" },
            { "data": "ftseESG", "width": "15%" },
            { "data": "issESG", "width": "15%" },
            { "data": "sapESG", "width": "15%" },
            { "data": "twCompanyRank", "width": "15%" },
            { "data": "refi", "width": "15%" }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.3/i18n/zh_Hant.json"
        }
    });

}