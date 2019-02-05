Loader = {
    checkQueryAsync: async function () {
        alert('checkQueryAsync');
    },
    fetchAsync: async function () {
        document.body.innerHTML = "<h1>HI</h1>";
    }
};

alert(action);