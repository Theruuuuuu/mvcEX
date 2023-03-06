

var datatable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else {
        if (url.includes("completed")) {
            loadDataTable("completed");
        }
        else {
            if (url.includes("pending")) {
                loadDataTable("pending");
            }
            else {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                }
                else {
                    loadDataTable("all");
                }
            }
        }
    }
});

function loadDataTable(status) {
    datatable = $('#tbltable').DataTable({
        /*取得資料*/
        "ajax": {
            "url": "/Admin/Order/GetAll?status=" + status
        },
        /*columns通常會是陣列*/
        "columns": [
            /*須注意變數名稱要和Json格式的變數名稱一致*/
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "25%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "10%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "10%" },
            {
                "data": "id",
                /*動態建立按鈕的HTML*/
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">							
							<a href="/Admin/Order/Details?orderId=${data}"
                                class="btn btn-primary mx-2"><i class="bi bi-pencil-square">Details</i>
							</a>							
						</div>
                        `
                },
                "width": "5%"
            }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.3/i18n/zh_Hant.json"
        }
    });
}
