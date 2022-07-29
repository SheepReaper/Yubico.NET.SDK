{
    global:
        Native_BN_bin2bn;
        Native_BN_bn2bin;
        Native_BN_clear_free;
        Native_BN_num_bytes;
        Native_EC_GROUP_free;
        Native_EC_GROUP_get_degree;
        Native_EC_GROUP_new_by_curve_name;
        Native_EC_KEY_free;
        Native_EC_KEY_get0_private_key;
        Native_EC_KEY_new_by_curve_name;
        Native_EC_KEY_set_private_key;
        Native_EC_POINT_free;
        Native_EC_POINT_get_affine_coordinates_GFp;
        Native_EC_POINT_mul;
        Native_EC_POINT_new;
        Native_EC_POINT_set_affine_coordinates_GFp;
        Native_ECDH_compute_key;
        Native_SCardBeginTransaction;
        Native_SCardCancel;
        Native_SCardConnect;
        Native_SCardDisconnect;
        Native_SCardEndTransaction;
        Native_SCardEstablishContext;
        Native_SCardGetStatusChange;
        Native_SCardListReaders;
        Native_SCardReconnect;
        Native_SCardReleaseContext;
        Native_SCardTransmit;

    local:
        *;
};
