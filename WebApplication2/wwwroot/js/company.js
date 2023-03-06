

var datatable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    datatable = $('#tblData').DataTable({
        /*取得資料*/
        "ajax": {
            "url":"/Admin/Company/GetAll"
        },
        /*columns通常會是陣列*/
        "columns": [
            /*須注意變數名稱要和Json格式的變數名稱一致*/
            { "data": "name", "width": "15%" },
            { "data": "streetAddress", "width": "15%" },
            { "data": "city", "width": "15%" },
            { "data": "state", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            {
                "data": "id",
                /*動態建立按鈕的HTML*/
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">							
							<a href="/Admin/Company/Upsert?id=${data}"
                                class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i>Edit
							</a>
							<a onClick=Delete('/Admin/Company/Delete/${data}')
                                class="btn btn-danger mx-auto" ><i class="bi bi-trash-fill"></i>Delete
							</a>
						</div>
                        `
                },
                "width": "15%"
            }
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.3/i18n/zh_Hant.json"
        }
    });
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        datatable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}